using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Enumerators;
using Waher.Script.Persistence.SQL.Processors;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data source formed through an (FULL [OUTER]|OUTER) JOIN of two sources.
	/// </summary>
	public class FullOuterJoinedSource : JoinedSource
	{
		/// <summary>
		/// Data source formed through an (FULL [OUTER]|OUTER) JOIN of two sources.
		/// </summary>
		/// <param name="Left">Left source</param>
		/// <param name="Right">Right source</param>
		/// <param name="Conditions">Conditions for join.</param>
		public FullOuterJoinedSource(IDataSource Left, IDataSource Right,
			ScriptNode Conditions)
			: base(Left, Right, Conditions)
		{
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Generic">If objects of type <see cref="GenericObject"/> should be returned.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public override async Task<IResultSetEnumerator> Find(int Offset, int Top, bool Generic, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			ScriptNode LeftWhere = await Reduce(this.Left, Where);
			KeyValuePair<VariableReference, bool>[] LeftOrder = await Reduce(this.Left, Order);

			IResultSetEnumerator LeftEnum = await this.Left.Find(0, int.MaxValue, Generic, LeftWhere, Variables, LeftOrder, Node);

			ScriptNode RightWhere = await Reduce(this.Right, this.Left, Where);

			LeftEnum = new LeftOuterJoinedSource.LeftOuterJoinEnumerator(LeftEnum, this.Left.Name, this.Right, this.Right.Name, Generic,
				this.Combine(RightWhere, this.Conditions), Variables, false);

			ScriptNode RightWhere2 = await Reduce(this.Right, Where);
			KeyValuePair<VariableReference, bool>[] RightOrder2 = await Reduce(this.Right, Order);

			IResultSetEnumerator RightEnum = await this.Right.Find(0, int.MaxValue, Generic, RightWhere2, Variables, RightOrder2, Node);

			ScriptNode LeftWhere2 = await Reduce(this.Left, this.Right, Where);

			RightEnum = new LeftOuterJoinedSource.LeftOuterJoinEnumerator(RightEnum, this.Right.Name, this.Left, this.Left.Name, Generic,
				this.Combine(LeftWhere2, this.Conditions), Variables, true);

			IResultSetEnumerator e = new FullOuterJoinEnumerator(LeftEnum, RightEnum);

			if (!(Where is null))
				e = new ConditionalEnumerator(e, Variables, Where);

			if (Offset > 0)
				e = new OffsetEnumerator(e, Offset);

			if (Top != int.MaxValue)
				e = new MaxCountEnumerator(e, Top);

			return e;
		}

		private class FullOuterJoinEnumerator : IResultSetEnumerator
		{
			private readonly Dictionary<object, bool> reportedLeft = new Dictionary<object, bool>();
			private readonly IResultSetEnumerator leftEnum;
			private readonly IResultSetEnumerator rightEnum;
			private object current;
			private bool leftMode;

			public FullOuterJoinEnumerator(IResultSetEnumerator Left, IResultSetEnumerator Right)
			{
				this.leftEnum = Left;
				this.rightEnum = Right;
				this.leftMode = true;
				this.current = null;
			}

			public object Current => this.current;

			public bool MoveNext()
			{
				return this.MoveNextAsync().Result;
			}

			public async Task<bool> MoveNextAsync()
			{
				if (this.leftMode)
				{
					if (await this.leftEnum.MoveNextAsync())
					{
						this.current = this.leftEnum.Current;
						this.reportedLeft[this.current] = true;
						return true;
					}
					else
						this.leftMode = false;
				}

				do
				{
					if (await this.rightEnum.MoveNextAsync())
						this.current = this.rightEnum.Current;
					else
						return false;
				}
				while (this.reportedLeft.ContainsKey(this.current));

				return true;
			}

			public void Reset()
			{
				this.reportedLeft.Clear();
				this.current = null;
				this.leftMode = true;
				this.leftEnum.Reset();
				this.rightEnum.Reset();
			}
		}

		/// <summary>
		/// Processes objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to process.</param>
		/// <param name="Generic">If objects of type <see cref="GenericObject"/> should be processed.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public override async Task<bool> Process(IProcessor<object> Processor, int Offset, int Top, bool Generic,
			ScriptNode Where, Variables Variables, KeyValuePair<VariableReference, bool>[] Order,
			ScriptNode Node)
		{
			ScriptNode LeftWhere = await Reduce(this.Left, Where);
			KeyValuePair<VariableReference, bool>[] LeftOrder = await Reduce(this.Left, Order);

			ScriptNode RightWhere = await Reduce(this.Right, this.Left, Where);
			RightWhere = this.Combine(RightWhere, this.Conditions);

			if (Top != int.MaxValue)
				Processor = new MaxCountProcessor(Processor, Top);

			if (Offset > 0)
				Processor = new OffsetProcessor(Processor, Offset);

			if (!(Where is null))
				Processor = new ConditionalProcessor(Processor, Variables, Where);

			FullOuterJoinProcessor OuterProcessor = new FullOuterJoinProcessor(Processor);

			LeftOuterJoinedSource.OuterJoinLeftProcessor LeftProcessor =
				 new LeftOuterJoinedSource.OuterJoinLeftProcessor(OuterProcessor, this.Left.Name,
					 this.Right, this.Right.Name, Generic, RightWhere, Variables, false);

			if (!await this.Left.Process(LeftProcessor, 0, int.MaxValue, Generic,
				LeftWhere, Variables, LeftOrder, Node))
			{
				return false;
			}

			RightWhere = await Reduce(this.Right, Where);
			KeyValuePair<VariableReference, bool>[] RightOrder = await Reduce(this.Right, Order);

			LeftWhere = await Reduce(this.Left, this.Right, Where);
			LeftWhere = this.Combine(LeftWhere, this.Conditions);

			LeftOuterJoinedSource.OuterJoinLeftProcessor RightProcessor =
				new LeftOuterJoinedSource.OuterJoinLeftProcessor(OuterProcessor, this.Right.Name,
					this.Left, this.Left.Name, Generic, LeftWhere, Variables, true);

			OuterProcessor.LeftMode = false;

			if (!await this.Right.Process(RightProcessor, 0, int.MaxValue, Generic,
				RightWhere, Variables, RightOrder, Node))
			{
				return false;
			}

			return true;
		}

		private class FullOuterJoinProcessor : IProcessor<object>
		{
			private readonly Dictionary<object, bool> reportedLeft = new Dictionary<object, bool>();
			private readonly IProcessor<object> processor;

			public FullOuterJoinProcessor(IProcessor<object> Processor)
			{
				this.processor = Processor;
			}

			public bool LeftMode { get; set; } = true;
			public bool IsAsynchronous => this.processor.IsAsynchronous;
			public bool Flush() => this.processor.Flush();
			public Task<bool> FlushAsync() => this.processor.FlushAsync();

			public bool Process(object Object)
			{
				if (this.LeftMode)
				{
					this.reportedLeft[Object] = true;
					return this.processor.Process(Object);
				}
				else
				{
					if (!this.reportedLeft.ContainsKey(Object))
						return this.processor.Process(Object);

					return true;
				}
			}

			public async Task<bool> ProcessAsync(object Object)
			{
				if (this.LeftMode)
				{
					this.reportedLeft[Object] = true;
					return await this.processor.ProcessAsync(Object);
				}
				else
				{
					if (!this.reportedLeft.ContainsKey(Object))
						return await this.processor.ProcessAsync(Object);
				
					return true;
				}
			}
		}

	}
}
