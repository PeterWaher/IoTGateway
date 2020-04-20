using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a SELECT statement against the object database.
	/// </summary>
	public class Select : ScriptNode
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private readonly ScriptNode[] sources;
		private readonly ScriptNode[] sourceNames;
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupByNames;
		private readonly KeyValuePair<ScriptNode, bool>[] orderBy;
		private readonly bool distinct;
		private ScriptNode top;
		private ScriptNode where;
		private ScriptNode having;
		private ScriptNode offset;

		/// <summary>
		/// Executes a SELECT statement against the object database.
		/// </summary>
		/// <param name="Columns">Columns to select. If null, all columns are selected.</param>
		/// <param name="ColumnNames">Optional renaming of columns.</param>
		/// <param name="Sources">Sources to select from.</param>
		/// <param name="SourceNames">Optional renaming of sources.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="GroupBy">Optional grouping</param>
		/// <param name="GroupByNames">Optional renaming of groups</param>
		/// <param name="Having">Optional Having clause</param>
		/// <param name="OrderBy">Optional ordering</param>
		/// <param name="Top">Optional limit on number of records to return</param>
		/// <param name="Offset">Optional offset into result set where reporting begins</param>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(ScriptNode[] Columns, ScriptNode[] ColumnNames, ScriptNode[] Sources, ScriptNode[] SourceNames, ScriptNode Where,
			ScriptNode[] GroupBy, ScriptNode[] GroupByNames, ScriptNode Having, KeyValuePair<ScriptNode, bool>[] OrderBy, ScriptNode Top,
			ScriptNode Offset, bool Distinct, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.columns = Columns;
			this.columnNames = ColumnNames;
			this.top = Top;
			this.sources = Sources;
			this.sourceNames = SourceNames;
			this.where = Where;
			this.groupBy = GroupBy;
			this.groupByNames = GroupByNames;
			this.having = Having;
			this.orderBy = OrderBy;
			this.offset = Offset;
			this.distinct = Distinct;

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("Columns and ColumnNames must both be null or not null.", nameof(ColumnNames));

			if (this.columns != null && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("Columns and ColumnNames must be of equal length.", nameof(ColumnNames));

			if (this.sources.Length != this.sourceNames.Length)
				throw new ArgumentException("Sources and SourceNames must be of equal length.", nameof(SourceNames));

			if (this.columnNames is null ^ this.columns is null)
				throw new ArgumentException("GroupBy and GroupByNames must both be null or not null.", nameof(GroupByNames));

			if (this.columns != null && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("GroupBy and GroupByNames must be of equal length.", nameof(GroupByNames));
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E;
			int Top;
			int Offset;
			int i, c;

			if (this.top != null)
			{
				E = this.top.Evaluate(Variables);
				Top = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Top <= 0)
					throw new ScriptRuntimeException("TOP must evaluate to a positive integer.", this.top);
			}
			else
				Top = int.MaxValue;

			if (this.offset != null)
			{
				E = this.offset.Evaluate(Variables);
				Offset = (int)Expression.ToDouble(E.AssociatedObjectValue);
				if (Offset < 0)
					throw new ScriptRuntimeException("OFFSET must evaluate to a non-negative integer.", this.offset);
			}
			else
				Offset = 0;

			c = this.sources.Length;
			if (c != 1)
				throw new ScriptRuntimeException("Joinds between multiple sources not supported.", this);

			E = this.sources[0].Evaluate(Variables);
			if (!(E.AssociatedObjectValue is Type T))
				throw new ScriptRuntimeException("Type expected.", this.sources[0]);

			List<string> OrderBy = new List<string>();
			bool CalculatedOrder = false;

			if (this.orderBy != null)
			{
				foreach (KeyValuePair<ScriptNode, bool> Node in this.orderBy)
				{
					if (Node.Key is VariableReference Ref)
					{
						if (Node.Value)
							OrderBy.Add(Ref.VariableName);
						else
							OrderBy.Add("-" + Ref.VariableName);
					}
					else
						CalculatedOrder = true;
				}
			}

			if (this.groupBy != null)
			{
				foreach (ScriptNode Node in this.groupBy)
				{
					if (Node is VariableReference Ref)
					{
						if (!OrderBy.Contains(Ref.VariableName))
							OrderBy.Add(Ref.VariableName);
					}
					else
						CalculatedOrder = true;
				}
			}

			LinkedList<IElement[]> Items = new LinkedList<IElement[]>();
			Dictionary<string, int> Columns = new Dictionary<string, int>();
			IEnumerator e;
			RecordEnumerator e2;
			int NrRecords = 0;

			if (this.columns != null)
			{
				c = this.columns.Length;
				for (i = 0; i < c; i++)
				{
					if (this.columns[i] is VariableReference Ref)
						Columns[Ref.VariableName] = i;
				}
			}
			else
				c = 0;

			if (this.groupBy is null)
			{
				e = Find(T, Offset, Top, this.where, Variables, OrderBy.ToArray(), this);
				Offset = 0;
				Top = int.MaxValue;
			}
			else
				e = Find(T, 0, int.MaxValue, this.where, Variables, OrderBy.ToArray(), this);

			if (this.where != null)
				e = new ConditionalEnumerator(e, Variables, this.where);

			if (this.groupBy != null)
			{
				e = new GroupEnumerator(e, Variables, this.groupBy, this.groupByNames);

				if (this.having != null)
					e = new ConditionalEnumerator(e, Variables, this.having);
			}

			if (CalculatedOrder)
			{
				List<KeyValuePair<ScriptNode, bool>> Order = new List<KeyValuePair<ScriptNode, bool>>();

				if (this.orderBy != null)
					Order.AddRange(this.orderBy);

				if (this.groupByNames != null)
				{
					foreach (ScriptNode Group in this.groupByNames)
					{
						if (Group != null)
							Order.Add(new KeyValuePair<ScriptNode, bool>(Group, true));
					}
				}

				e = new CustomOrderEnumerator(e, Variables, Order.ToArray());
			}

			if (Offset > 0)
				e = new OffsetEnumerator(e, Offset);

			if (Top != int.MaxValue)
				e = new MaxCountEnumerator(e, Top);

			if (this.distinct)
				e2 = new DistinctEnumerator(e, this.columns, Variables);
			else
				e2 = new RecordEnumerator(e, this.columns, Variables);

			while (e2.MoveNext())
			{
				Items.AddLast(e2.CurrentRecord);
				NrRecords++;
			}

			IElement[] Elements = new IElement[this.columns is null ? NrRecords : NrRecords * c];

			i = 0;
			foreach (IElement[] Record in Items)
			{
				foreach (IElement Item in Record)
					Elements[i++] = Item;

				if (c > 0)
				{
					while (i % c != 0)
						Elements[i++] = new ObjectValue(null);
				}
			}

			if (Elements.Length == 1)
				return Elements[0];
			else if (this.columns is null)
				return Operators.Vectors.VectorDefinition.Encapsulate(Elements, false, this);

			ObjectMatrix Result = new ObjectMatrix(NrRecords, c, Elements);
			string[] Names = new string[c];

			foreach (KeyValuePair<string, int> P in Columns)
				Names[P.Value] = P.Key;

			i = 0;
			if (this.columnNames != null)
			{
				while (i < c)
				{
					if (this.columnNames[i] is VariableReference Ref)
						Names[i++] = Ref.VariableName;
					else
					{
						E = this.columnNames[i]?.Evaluate(Variables);
						if (E is null)
						{
							if (Names[i] is null)
								Names[i] = (i + 1).ToString();

							i++;
						}
						else if (E is StringValue S)
							Names[i++] = S.Value;
						else
							Names[i++] = Expression.ToString(E.AssociatedObjectValue);
					}
				}
			}

			while (i < c)
			{
				if (Names[i] is null)
					Names[i] = (i + 1).ToString();

				i++;
			}

			Result.ColumnNames = Names;

			return Result;

			// TODO: Joins
			// TODO: Source names
		}

		internal static IEnumerator Find(Type T, int Offset, int Top, ScriptNode Where, Variables Variables, string[] Order, ScriptNode Node)
		{
			object[] FindParameters = new object[] { Offset, Top, Convert(Where, Variables), Order };
			object Obj = FindMethod.MakeGenericMethod(T).Invoke(null, FindParameters);
			if (!(Obj is Task Task))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			PropertyInfo PI = Task.GetType().GetRuntimeProperty("Result");
			if (PI is null)
				throw new ScriptRuntimeException("Unexpected response.", Node);

			Obj = PI.GetValue(Task);
			if (!(Obj is IEnumerable Enumerable))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			return Enumerable.GetEnumerator();
		}

		private static MethodInfo findMethod = null;

		/// <summary>
		/// Generic object database Find method: <see cref="Database.Find{T}(int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo FindMethod
		{
			get
			{
				if (findMethod is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("Find"))
					{
						if (!MI.ContainsGenericParameters)
							continue;

						ParameterInfo[] Parameters = MI.GetParameters();
						if (Parameters.Length != 4 ||
							Parameters[0].ParameterType != typeof(int) ||
							Parameters[1].ParameterType != typeof(int) ||
							Parameters[2].ParameterType != typeof(Filter) ||
							Parameters[3].ParameterType != typeof(string[]))
						{
							continue;
						}

						findMethod = MI;
					}

					if (findMethod is null)
						throw new InvalidOperationException("Appropriate Database.Find method not found.");
				}

				return findMethod;
			}
		}

		internal static Filter Convert(ScriptNode Conditions, Variables Variables)
		{
			if (Conditions is null)
				return null;

			Operators.Logical.And And = Conditions as Operators.Logical.And;
			Operators.Dual.And And2 = And is null ? Conditions as Operators.Dual.And : null;

			if (And != null || And2 != null)
			{
				Filter L = Convert(And != null ? And.LeftOperand : And2.LeftOperand, Variables);
				Filter R = Convert(And != null ? And.RightOperand : And2.RightOperand, Variables);

				if (L is null && R is null)
					return null;
				else if (L is null)
					return R;
				else if (R is null)
					return L;
				else
				{
					List<Filter> Filters = new List<Filter>();

					if (L is FilterAnd L2)
						Filters.AddRange(L2.ChildFilters);
					else
						Filters.Add(L);

					if (R is FilterAnd R2)
						Filters.AddRange(R2.ChildFilters);
					else
						Filters.Add(R);

					return new FilterAnd(Filters.ToArray());
				}
			}

			Operators.Logical.Or Or = Conditions as Operators.Logical.Or;
			Operators.Dual.Or Or2 = Or is null ? Conditions as Operators.Dual.Or : null;

			if (Or != null || Or2 != null)
			{
				Filter L = Convert(Or != null ? Or.LeftOperand : Or2.LeftOperand, Variables);
				Filter R = Convert(Or != null ? Or.RightOperand : Or2.RightOperand, Variables);

				if (L is null || R is null)
					return null;
				else
				{
					List<Filter> Filters = new List<Filter>();

					if (L is FilterOr L2)
						Filters.AddRange(L2.ChildFilters);
					else
						Filters.Add(L);

					if (R is FilterOr R2)
						Filters.AddRange(R2.ChildFilters);
					else
						Filters.Add(R);

					return new FilterOr(Filters.ToArray());
				}
			}

			if (Conditions is Operators.Logical.Not Not)
			{
				Filter F = Convert(Not.Operand, Variables);
				if (F is null)
					return null;
				else
					return new FilterNot(F);
			}
			else if (Conditions is BinaryOperator Bin && Bin.LeftOperand is VariableReference Var)
			{
				string FieldName = Var.VariableName;
				object Value = Bin.RightOperand.Evaluate(Variables)?.AssociatedObjectValue ?? null;

				if (Conditions is Operators.Comparisons.EqualTo ||
					Conditions is Operators.Comparisons.EqualToElementWise ||
					Conditions is Operators.Comparisons.IdenticalTo ||
					Conditions is Operators.Comparisons.IdenticalToElementWise)
				{
					return new FilterFieldEqualTo(FieldName, Value);
				}
				else if (Conditions is Operators.Comparisons.NotEqualTo ||
					Conditions is Operators.Comparisons.NotEqualToElementWise)
				{
					return new FilterFieldNotEqualTo(FieldName, Value);
				}
				else if (Conditions is Operators.Comparisons.GreaterThan)
					return new FilterFieldGreaterThan(FieldName, Value);
				else if (Conditions is Operators.Comparisons.GreaterThanOrEqualTo)
					return new FilterFieldGreaterOrEqualTo(FieldName, Value);
				else if (Conditions is Operators.Comparisons.LesserThan)
					return new FilterFieldLesserThan(FieldName, Value);
				else if (Conditions is Operators.Comparisons.LesserThanOrEqualTo)
					return new FilterFieldLesserOrEqualTo(FieldName, Value);
				else if (Conditions is Operators.Comparisons.Like Like)
				{
					string RegEx = WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
					Like.TransformExpression += (Expression) => WildcardToRegex(Expression, "%");
					return new FilterFieldLikeRegEx(FieldName, RegEx);
				}
				else if (Conditions is Operators.Comparisons.NotLike NotLike)
				{
					string RegEx = WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
					NotLike.TransformExpression += (Expression) => WildcardToRegex(Expression, "%");
					return new FilterNot(new FilterFieldLikeRegEx(FieldName, RegEx));
				}
			}

			return null;
		}

		/// <summary>
		/// Converts a wildcard string to a regular expression string.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Wildcard">Wildcardd</param>
		/// <returns>Regular expression</returns>
		public static string WildcardToRegex(string s, string Wildcard)
		{
			string[] Parts = s.Split(new string[] { Wildcard }, StringSplitOptions.None);
			StringBuilder RegEx = new StringBuilder();
			bool First = true;
			int i, j, c;

			foreach (string Part in Parts)
			{
				if (First)
					First = false;
				else
					RegEx.Append(".*");

				i = 0;
				c = Part.Length;
				while (i < c)
				{
					j = Part.IndexOfAny(regexSpecialCharaters, i);
					if (j < i)
					{
						RegEx.Append(Part.Substring(i));
						i = c;
					}
					else
					{
						if (j > i)
							RegEx.Append(Part.Substring(i, j - i));

						RegEx.Append('\\');
						RegEx.Append(Part[j]);

						i = j + 1;
					}
				}
			}

			return RegEx.ToString();
		}

		private static readonly char[] regexSpecialCharaters = new char[] { '\\', '^', '$', '{', '}', '[', ']', '(', ')', '.', '*', '+', '?', '|', '<', '>', '-', '&' };

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			int i, c;

			if (DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.sources, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.sourceNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst) ||
					!(this.top?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}

				if (this.orderBy != null)
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
							return false;
					}
				}
			}

			if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.sources, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.sourceNames, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
				!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst))
			{
				return false;
			}

			if (!(this.top is null) && !Callback(ref this.top, State))
				return false;

			if (!(this.where is null) && !Callback(ref this.where, State))
				return false;

			if (!(this.having is null) && !Callback(ref this.having, State))
				return false;

			if (!(this.offset is null) && !Callback(ref this.offset, State))
				return false;

			if (this.orderBy != null)
			{
				c = this.orderBy.Length;
				for (i = 0; i < c; i++)
				{
					ScriptNode Node = this.orderBy[i].Key;
					if (!(Node is null))
					{
						ScriptNode Node0 = Node;

						if (!Callback(ref Node, State))
							return false;

						if (Node != Node0)
							this.orderBy[i] = new KeyValuePair<ScriptNode, bool>(Node, this.orderBy[i].Value);
					}
				}
			}

			if (!DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.columns, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.columnNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.sources, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.sourceNames, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupBy, State, DepthFirst) ||
					!ForAllChildNodes(Callback, this.groupByNames, State, DepthFirst) ||
					!(this.top?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.where?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.having?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.offset?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}

				if (this.orderBy != null)
				{
					c = this.orderBy.Length;
					for (i = 0; i < c; i++)
					{
						if (!(this.orderBy[i].Key?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
							return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is Select O &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.columnNames, O.columnNames) &&
				AreEqual(this.sources, O.sources) &&
				AreEqual(this.sourceNames, O.sourceNames) &&
				AreEqual(this.groupBy, O.groupBy) &&
				AreEqual(this.groupByNames, O.groupByNames) &&
				AreEqual(this.top, O.top) &&
				AreEqual(this.where, O.where) &&
				AreEqual(this.having, O.having) &&
				AreEqual(this.offset, O.offset) &&
				this.distinct == O.distinct &&
				base.Equals(obj)))
			{
				return false;
			}

			IEnumerator e1 = this.orderBy.GetEnumerator();
			IEnumerator e2 = O.orderBy.GetEnumerator();

			while (true)
			{
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();

				if (b1 ^ b2)
					return false;

				if (!b1)
					return true;

				KeyValuePair<ScriptNode, bool> Item1 = (KeyValuePair<ScriptNode, bool>)e1.Current;
				KeyValuePair<ScriptNode, bool> Item2 = (KeyValuePair<ScriptNode, bool>)e2.Current;

				if (!Item1.Key.Equals(Item2.Key) ||
					!Item1.Value.Equals(Item2.Value))
				{
					return false;
				}
			}
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.columnNames);
			Result ^= Result << 5 ^ GetHashCode(this.sources);
			Result ^= Result << 5 ^ GetHashCode(this.sourceNames);
			Result ^= Result << 5 ^ GetHashCode(this.groupBy);
			Result ^= Result << 5 ^ GetHashCode(this.groupByNames);
			Result ^= Result << 5 ^ GetHashCode(this.top);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			Result ^= Result << 5 ^ GetHashCode(this.having);
			Result ^= Result << 5 ^ GetHashCode(this.offset);
			Result ^= Result << 5 ^ this.distinct.GetHashCode();

			foreach (KeyValuePair<ScriptNode, bool> P in this.orderBy)
			{
				Result ^= Result << 5 ^ P.Key.GetHashCode();
				Result ^= Result << 5 ^ P.Value.GetHashCode();
			}

			return Result;
		}

	}
}
