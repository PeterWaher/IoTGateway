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

		/// <summary>
		/// Abstract base classes of joined sources.
		/// </summary>
		/// <param name="Left">Left source</param>
		/// <param name="Right">Right source</param>
		/// <param name="Conditions">Conditions for join.</param>
		public JoinedSource(IDataSource Left, IDataSource Right, ScriptNode Conditions)
		{
			this.left = Left;
			this.right = Right;
			this.conditions = Conditions;
		}

		/// <summary>
		/// Left source
		/// </summary>
		public IDataSource Left => this.left;

		/// <summary>
		/// Right source
		/// </summary>
		public IDataSource Right => this.right;

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
		/// Finds and Deletes a set of objects.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		public Task<int> FindDelete(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
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
		/// Collection name or alias.
		/// </summary>
		public string Name
		{
			get => string.Empty;
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
				return new Operators.Logical.And(Where, On, 0, 0, Where.Expression);
		}

		/// <summary>
		/// Reduces a where clause to fit the current data source.
		/// </summary>
		/// <param name="Source">Data Source to which the expression is to be reduced.</param>
		/// <param name="Where">Where clause</param>
		/// <returns>Reduced where clause, fitting the data source.</returns>
		protected static async Task<ScriptNode> Reduce(IDataSource Source, ScriptNode Where)
		{
			KeyValuePair<ScriptNode, int> P = await Reduce(Source, null, Where, 1);
			return P.Key;
		}

		/// <summary>
		/// Reduces a where clause to fit the current data sources.
		/// </summary>
		/// <param name="Source">Data Source to which the expression is to be reduced.</param>
		/// <param name="Source2">Optional second source.</param>
		/// <param name="Where">Where clause</param>
		/// <returns>Reduced where clause, fitting the data sources.</returns>
		protected static async Task<ScriptNode> Reduce(IDataSource Source, IDataSource Source2, ScriptNode Where)
		{
			KeyValuePair<ScriptNode, int> P = await Reduce(Source, Source2, Where, 1);
			return P.Key;
		}

		/// <summary>
		/// Reduces a where clause to fit the current data source.
		/// </summary>
		/// <param name="Source">Data Source to which the expression is to be reduced.</param>
		/// <param name="Source2">Optional second source.</param>
		/// <param name="Where">Where clause</param>
		/// <param name="Mask">Source mask.</param>
		/// <returns>Reduced where clause, fitting the data source.</returns>
		private static async Task<KeyValuePair<ScriptNode, int>> Reduce(IDataSource Source, IDataSource Source2, ScriptNode Where, int Mask)
		{
			KeyValuePair<ScriptNode, int> P1, P2;
			ScriptNode Op1;
			ScriptNode Op2;
			int s1;
			int s2;
			int s;

			if (Where is null)
				return Null;
			else if (Where is BinaryOperator BinOp)
			{
				P1 = await Reduce(Source, Source2, BinOp.LeftOperand, Mask);
				Op1 = P1.Key;
				s1 = P1.Value;

				P2 = await Reduce(Source, Source2, BinOp.RightOperand, Mask);
				Op2 = P2.Key;
				s2 = P2.Value;

				s = s1 | s2;

				if ((s & Mask) == 0)
					return Null;

				bool IsAnd = BinOp is Operators.Logical.And || BinOp is Operators.Dual.And;
				bool IsOr = !IsAnd && (Where is Operators.Logical.Or || Where is Operators.Dual.Or);

				if (IsAnd || IsOr)
				{
					if (Op1 is null)
						return P2;
					else if (Op2 is null)
						return P1;
					else if (IsAnd)
						return new KeyValuePair<ScriptNode, int>(new Operators.Logical.And(Op1, Op2, 0, 0, BinOp.Expression), s);
					else if (IsOr)
						return new KeyValuePair<ScriptNode, int>(new Operators.Logical.Or(Op1, Op2, 0, 0, BinOp.Expression), s);
				}
				else if (Op1 is null || Op2 is null)
					return Null;
				else if (Op1 == BinOp.LeftOperand && Op2 == BinOp.RightOperand)
					return new KeyValuePair<ScriptNode, int>(Where, s);
				else if (BinOp is Operators.Comparisons.EqualTo)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.EqualTo(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.GreaterThan)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.GreaterThan(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.GreaterThanOrEqualTo)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.GreaterThanOrEqualTo(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.LesserThan)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.LesserThan(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.LesserThanOrEqualTo)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.LesserThanOrEqualTo(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.Like)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.Like(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.NotEqualTo)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.NotEqualTo(Op1, Op2, 0, 0, BinOp.Expression), s);
				else if (BinOp is Operators.Comparisons.NotLike)
					return new KeyValuePair<ScriptNode, int>(new Operators.Comparisons.NotLike(Op1, Op2, 0, 0, BinOp.Expression), s);
			}
			else if (Where is Operators.Membership.NamedMember N)
			{
				if (N.Operand is VariableReference Ref)
				{
					if (Source.IsSource(Ref.VariableName))
						return new KeyValuePair<ScriptNode, int>(N, 1);
					else if (Source2?.IsSource(Ref.VariableName) ?? false)
						return new KeyValuePair<ScriptNode, int>(N, 2);
					else
						return Null;
				}
				else
					return Null;
			}
			else if (Where is UnaryOperator UnOp)
			{
				P1 = await Reduce(Source, Source2, UnOp.Operand, Mask);
				Op1 = P1.Key;
				s1 = P1.Value;

				if (Op1 is null)
					return Null;

				if (UnOp is Operators.Logical.Not)
					return new KeyValuePair<ScriptNode, int>(new Operators.Logical.Not(Op1, 0, 0, UnOp.Expression), s1);
			}
			else if (Where is VariableReference Ref)
			{
				if (await Source.IsLabel(Ref.VariableName))
					return new KeyValuePair<ScriptNode, int>(Ref, 1);
				else if (!(Source2 is null) && await Source2.IsLabel(Ref.VariableName))
					return new KeyValuePair<ScriptNode, int>(Ref, 2);
				else
					return Null;
			}
			else if (Where is ConstantElement C)
				return new KeyValuePair<ScriptNode, int>(C, 0);

			return Null;
		}

		private static readonly KeyValuePair<ScriptNode, int> Null = new KeyValuePair<ScriptNode, int>(null, 0);

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

		/// <summary>
		/// Creates an index in the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <param name="Fields">Field names. Prefix with hyphen (-) to define descending order.</param>
		public Task CreateIndex(string Name, string[] Fields)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Drops an index from the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <returns>If an index was found and dropped.</returns>
		public Task<bool> DropIndex(string Name)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Drops the collection from the source.
		/// </summary>
		public Task DropCollection()
		{
			throw InvalidOperation();
		}

	}
}
