using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Abstract base classes of joined sources.
	/// </summary>
	public abstract class JoinedSource : IDataSource
	{
		private readonly IDataSource left;
		private readonly IDataSource right;
		private readonly ScriptNode conditions;
		private readonly string leftName;
		private readonly string rightName;

		/// <summary>
		/// Abstract base classes of joined sources.
		/// </summary>
		/// <param name="Left">Left source</param>
		/// <param name="LeftName">Name (or alias) of left source.</param>
		/// <param name="Right">Right source</param>
		/// <param name="RightName">Name (or alias) of right source.</param>
		/// <param name="Conditions">Conditions for join.</param>
		public JoinedSource(IDataSource Left, string LeftName, 
			IDataSource Right, string RightName, ScriptNode Conditions)
		{
			this.left = Left;
			this.leftName = LeftName;
			this.right = Right;
			this.rightName = RightName;
			this.conditions = Conditions;
		}

		/// <summary>
		/// Left source
		/// </summary>
		public IDataSource Left => this.left;

		/// <summary>
		/// Name (or alias) of left source.
		/// </summary>
		public string LeftName => this.leftName;

		/// <summary>
		/// Right source
		/// </summary>
		public IDataSource Right => this.right;

		/// <summary>
		/// Name (or alias) of right source.
		/// </summary>
		public string RightName => this.rightName;

		/// <summary>
		/// Conditions for join.
		/// </summary>
		public ScriptNode Conditions => this.conditions;

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
		public abstract Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node);

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		public Task Update(IEnumerable<object> Objects)
		{
			throw InvalidOperation();
		}

		private static Exception InvalidOperation()
		{
			return new InvalidOperationException("Operation not permitted on joined sources.");
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(object Object)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get => throw InvalidOperation();
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get => throw InvalidOperation();
		}

		/// <summary>
		/// Checks if the name refers to the source.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name refers to the source.</returns>
		public bool IsSource(string Name)
		{
			return this.left.IsSource(Name) || this.right.IsSource(Name);
		}

		/// <summary>
		/// Checks if the label is a label in the source.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>If the label is a label in the source.</returns>
		public async Task<bool> IsLabel(string Label)
		{
			return 
				await this.left.IsLabel(Label) || 
				await this.right.IsLabel(Label);
		}

		/// <summary>
		/// Combines one or two restrictions.
		/// </summary>
		/// <param name="Where">WHERE clause.</param>
		/// <param name="On">ON clause.</param>
		/// <returns>Combined restrictions.</returns>
		protected ScriptNode Combine(ScriptNode Where, ScriptNode On)
		{
			if (Where is null)
				return On;
			else if (On is null)
				return Where;
			else
				return new Operators.Logical.And(Where, On, Where.Start, Where.Length, Where.Expression);
		}

		/// <summary>
		/// Reduces a where clause to fit the current data source.
		/// </summary>
		/// <param name="Source">Data Source to which the expression is to be reduced.</param>
		/// <param name="Where">Where clause</param>
		/// <returns>Reduced where clause, fitting the data source.</returns>
		protected static async Task<ScriptNode> Reduce(IDataSource Source, ScriptNode Where)
		{
			ScriptNode Op1;
			ScriptNode Op2;

			if (Where is null)
				return null;
			else if (Where is BinaryOperator BinOp)
			{
				Op1 = await Reduce(Source, BinOp.LeftOperand);
				Op2 = await Reduce(Source, BinOp.RightOperand);

				bool IsAnd = BinOp is Operators.Logical.And || BinOp is Operators.Dual.And;
				bool IsOr = !IsAnd && (Where is Operators.Logical.Or || Where is Operators.Dual.Or);

				if (IsAnd || IsOr)
				{
					if (Op1 is null)
						return Op2;
					else if (Op2 is null)
						return Op1;
					else if (IsAnd)
						return new Operators.Logical.And(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
					else if (IsOr)
						return new Operators.Logical.Or(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				}
				else if (Op1 is null || Op2 is null)
					return null;
				else if (Op1 == BinOp.LeftOperand && Op2 == BinOp.RightOperand)
					return Where;
				else if (BinOp is Operators.Comparisons.EqualTo)
					return new Operators.Comparisons.EqualTo(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.GreaterThan)
					return new Operators.Comparisons.GreaterThan(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.GreaterThanOrEqualTo)
					return new Operators.Comparisons.GreaterThanOrEqualTo(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.LesserThan)
					return new Operators.Comparisons.LesserThan(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.LesserThanOrEqualTo)
					return new Operators.Comparisons.LesserThanOrEqualTo(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.Like)
					return new Operators.Comparisons.Like(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.NotEqualTo)
					return new Operators.Comparisons.NotEqualTo(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
				else if (BinOp is Operators.Comparisons.NotLike)
					return new Operators.Comparisons.NotLike(Op1, Op2, BinOp.Start, BinOp.Length, BinOp.Expression);
			}
			else if (Where is UnaryOperator UnOp)
			{
				Op1 = await Reduce(Source, UnOp.Operand);
				if (Op1 is null)
					return null;

				if (UnOp is Operators.Logical.Not)
					return new Operators.Logical.Not(Op1, UnOp.Start, UnOp.Length, UnOp.Expression);
			}
			else if (Where is Operators.Membership.NamedMember N)
			{
				if (N.Operand is VariableReference Ref)
				{
					if (Source.IsSource(Ref.VariableName))
						return N;
					else
						return null;
				}
				else
					return null;
			}
			else if (Where is VariableReference Ref)
			{
				if (await Source.IsLabel(Ref.VariableName))
					return Ref;
				else
					return null;
			}

			return Where;
		}

		/// <summary>
		/// Reduces a sort order clause to fit the current data source.
		/// </summary>
		/// <param name="Source">Data Source to which the order is to be reduced.</param>
		/// <param name="Order">Sort order</param>
		/// <returns>Reduced sort order.</returns>
		protected static async Task<KeyValuePair<VariableReference, bool>[]> Reduce(IDataSource Source,
			KeyValuePair<VariableReference, bool>[] Order)
		{
			if (Order is null)
				return null;

			int i, c = Order.Length;

			for (i = 0; i < c; i++)
			{
				if (!await Source.IsLabel(Order[i].Key.VariableName))
					break;
			}

			if (i != c)
				Array.Resize<KeyValuePair<VariableReference, bool>>(ref Order, c);

			return Order;
		}

	}
}
