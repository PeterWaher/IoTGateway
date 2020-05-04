using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Script.Model;

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
		/// <param name="LeftName">Name (or alias) of left source.</param>
		/// <param name="Right">Right source</param>
		/// <param name="RightName">Name (or alias) of right source.</param>
		/// <param name="Conditions">Conditions for join.</param>
		public InnerJoinedSource(IDataSource Left, string LeftName, 
			IDataSource Right, string RightName, ScriptNode Conditions)
			: base(Left, LeftName, Right, RightName, Conditions)
		{
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public override async Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			IResultSetEnumerator e = await this.Left.Find(0, int.MaxValue,
				await Reduce(this.Left, Where), Variables,
				await Reduce(this.Left, Order), Node);

			e = new InnerJoinEnumerator(e, this.LeftName, this.Right, this.RightName,
				this.Combine(await Reduce(this.Right, Where), this.Conditions), Variables);

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
			private readonly ScriptNode rightConditions;
			private readonly Variables variables;
			private readonly string leftName;
			private readonly string rightName;
			private readonly bool hasLeftName;
			private IResultSetEnumerator right;
			private JoinedObject current = null;

			public InnerJoinEnumerator(IResultSetEnumerator Left, string LeftName,
				IDataSource RightSource, string RightName, ScriptNode RightConditions,
				Variables Variables)
			{
				this.left = Left;
				this.leftName = LeftName;
				this.rightName = RightName;
				this.rightSource = RightSource;
				this.rightConditions = RightConditions;
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

					ObjectProperties LeftVariables = new ObjectProperties(this.left.Current, this.variables);

					if (this.hasLeftName)
						LeftVariables[this.leftName] = this.left.Current;

					this.right = await this.rightSource.Find(0, int.MaxValue, this.rightConditions, LeftVariables,
						null, this.rightConditions);
				}
			}

			public void Reset()
			{
				this.current = null;
				this.right = null;
				this.left.Reset();
			}
		}
	
	}
}
