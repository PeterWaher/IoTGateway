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
	/// Data source formed through an INNER JOIN of two sources.
	/// </summary>
	public class InnerJoinedSource : JoinedSource
	{
		/// <summary>
		/// Data source formed through an INNER JOIN of two sources.
		/// </summary>
		/// <param name="Left">Left source</param>
		/// <param name="Right">Right source</param>
		/// <param name="Conditions">Conditions for join.</param>
		public InnerJoinedSource(IDataSource Left, IDataSource Right, ScriptNode Conditions)
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

			IResultSetEnumerator e = await this.Left.Find(0, int.MaxValue, Generic, LeftWhere, Variables, LeftOrder, Node);

			ScriptNode RightWhere = await Reduce(this.Right, this.Left, Where);
			RightWhere = this.Combine(RightWhere, this.Conditions);

			e = new InnerJoinEnumerator(e, this.Left.Name, this.Right, this.Right.Name, Generic, RightWhere, Variables);

			if (!(Where is null))
				e = new ConditionalEnumerator(e, Variables, Where);

			if (Offset > 0)
				e = new OffsetEnumerator(e, Offset);

			if (Top != int.MaxValue)
				e = new MaxCountEnumerator(e, Top);

			return e;
		}

		private class InnerJoinEnumerator : IResultSetEnumerator
		{
			private readonly IResultSetEnumerator left;
			private readonly IDataSource rightSource;
			private readonly ScriptNode conditions;
			private readonly Variables variables;
			private readonly string leftName;
			private readonly string rightName;
			private readonly bool hasLeftName;
			private readonly bool generic;
			private IResultSetEnumerator right;
			private JoinedObject current = null;
			private ObjectProperties leftVariables = null;

			public InnerJoinEnumerator(IResultSetEnumerator Left, string LeftName, IDataSource RightSource, string RightName,
				bool Generic, ScriptNode Conditions, Variables Variables)
			{
				this.left = Left;
				this.leftName = LeftName;
				this.rightName = RightName;
				this.rightSource = RightSource;
				this.generic = Generic;
				this.conditions = Conditions;
				this.variables = Variables;
				this.hasLeftName = !string.IsNullOrEmpty(this.leftName);
			}

			public object Current => this.current;

			public bool MoveNext()
			{
				return this.MoveNextAsync().Result;
			}

			public async Task<bool> MoveNextAsync()
			{
				while (true)
				{
					if (!(this.right is null))
					{
						if (await this.right.MoveNextAsync())
						{
							this.current = new JoinedObject(this.left.Current, this.leftName,
								this.right.Current, this.rightName);

							return true;
						}
						else
							this.right = null;
					}

					if (!await this.left.MoveNextAsync())
						return false;

					if (this.leftVariables is null)
						this.leftVariables = new ObjectProperties(this.left.Current, this.variables);
					else
						this.leftVariables.Object = this.left.Current;

					if (this.hasLeftName)
						this.leftVariables[this.leftName] = this.left.Current;

					this.right = await this.rightSource.Find(0, int.MaxValue, this.generic, this.conditions, this.leftVariables,
						null, this.conditions);
				}
			}

			public void Reset()
			{
				this.current = null;
				this.right = null;
				this.left.Reset();
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

			Processor = new InnerJoinLeftProcessor(Processor, this.Left.Name, this.Right,
				this.Right.Name, Generic, RightWhere, Variables);

			return await this.Left.Process(Processor, 0, int.MaxValue, Generic,
				LeftWhere, Variables, LeftOrder, Node);
		}

		private class InnerJoinLeftProcessor : IProcessor<object>
		{
			private readonly InnerJoinRightProcessor rightProcessor;
			private readonly IDataSource rightSource;
			private readonly ScriptNode conditions;
			private readonly Variables variables;
			private readonly string leftName;
			private readonly bool hasLeftName;
			private readonly bool generic;
			private ObjectProperties leftVariables = null;

			public InnerJoinLeftProcessor(IProcessor<object> Processor, string LeftName,
				IDataSource RightSource, string RightName, bool Generic,
				ScriptNode Conditions, Variables Variables)
			{
				this.rightProcessor = new InnerJoinRightProcessor(Processor, LeftName, RightName);
				this.leftName = LeftName;
				this.rightSource = RightSource;
				this.generic = Generic;
				this.conditions = Conditions;
				this.variables = Variables;
				this.hasLeftName = !string.IsNullOrEmpty(this.leftName);
			}

			public bool IsAsynchronous => true;
			public bool Process(object Object) => this.ProcessAsync(Object).Result;
			public bool Flush() => this.rightProcessor.Flush();
			public Task<bool> FlushAsync() => this.rightProcessor.FlushAsync();

			public async Task<bool> ProcessAsync(object Object)
			{
				if (this.leftVariables is null)
					this.leftVariables = new ObjectProperties(Object, this.variables);
				else
					this.leftVariables.Object = Object;

				if (this.hasLeftName)
					this.leftVariables[this.leftName] = Object;

				this.rightProcessor.CurrentLeft = Object;

				return await this.rightSource.Process(this.rightProcessor, 0, int.MaxValue,
					this.generic, this.conditions, this.leftVariables, null, this.conditions);
			}
		}

		private class InnerJoinRightProcessor : IProcessor<object>
		{
			private readonly IProcessor<object> processor;
			private readonly string leftName;
			private readonly string rightName;

			public InnerJoinRightProcessor(IProcessor<object> Processor, string LeftName,
				string RightName)
			{
				this.processor = Processor;
				this.leftName = LeftName;
				this.rightName = RightName;
			}

			public object CurrentLeft { get; set; }

			public bool IsAsynchronous => this.processor.IsAsynchronous;
			public bool Flush() => this.processor.Flush();
			public Task<bool> FlushAsync() => this.processor.FlushAsync();

			public bool Process(object Object)
			{
				return this.processor.Process(new JoinedObject(this.CurrentLeft, 
					this.leftName, Object, this.rightName));
			}

			public Task<bool> ProcessAsync(object Object)
			{
				return this.processor.ProcessAsync(new JoinedObject(this.CurrentLeft, 
					this.leftName, Object, this.rightName));
			}
		}

	}
}