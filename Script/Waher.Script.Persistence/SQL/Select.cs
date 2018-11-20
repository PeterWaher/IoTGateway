using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
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
		private readonly ScriptNode top;
		private readonly ScriptNode where;
		private readonly ScriptNode having;
		private readonly ScriptNode offset;

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
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(ScriptNode[] Columns, ScriptNode[] ColumnNames, ScriptNode[] Sources, ScriptNode[] SourceNames, ScriptNode Where,
			ScriptNode[] GroupBy, ScriptNode[] GroupByNames, ScriptNode Having, KeyValuePair<ScriptNode, bool>[] OrderBy, ScriptNode Top,
			ScriptNode Offset, int Start, int Length, Expression Expression)
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

			if (this.columnNames == null ^ this.columns == null)
				throw new ArgumentException("Columns and ColumnNames must both be null or not null.", nameof(ColumnNames));

			if (this.columns != null && this.columns.Length != this.columnNames.Length)
				throw new ArgumentException("Columns and ColumnNames must be of equal length.", nameof(ColumnNames));

			if (this.sources.Length != this.sourceNames.Length)
				throw new ArgumentException("Sources and SourceNames must be of equal length.", nameof(SourceNames));

			if (this.columnNames == null ^ this.columns == null)
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

			if (findMethod == null)
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

				if (findMethod == null)
					throw new ScriptRuntimeException("Appropriate Database.Find method not found.", this);
			}

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

			MethodInfo Find = findMethod.MakeGenericMethod(T);

			object[] FindParameters = new object[] { Offset, Top, Convert(this.where, Variables), OrderBy.ToArray() };
			bool ManualPaging = this.groupBy != null || this.where != null;
			bool ManualTop = Top != int.MaxValue && ManualPaging;
			bool ManualOffset = Offset != 0 && ManualPaging;

			if (ManualTop)
				FindParameters[1] = int.MaxValue;

			if (ManualOffset)
				FindParameters[0] = 0;

			object Obj = Find.Invoke(null, FindParameters);
			if (!(Obj is Task Task))
				throw new ScriptRuntimeException("Unexpected response.", this);

			PropertyInfo PI = Task.GetType().GetRuntimeProperty("Result");
			if (PI == null)
				throw new ScriptRuntimeException("Unexpected response.", this);

			Obj = PI.GetValue(Task);
			if (!(Obj is IEnumerable Enumerable))
				throw new ScriptRuntimeException("Unexpected response.", this);

			IEnumerator e = Enumerable.GetEnumerator();
			LinkedList<IElement[]> Items = new LinkedList<IElement[]>();
			Dictionary<string, int> Columns = new Dictionary<string, int>();
			IElement[] Rec;
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

			Type LastItemType = null;

			while (e.MoveNext())
			{
				if (ManualOffset && Offset > 0)
				{
					Offset--;
					continue;
				}

				object Item = e.Current;
				ObjectProperties Properties = new ObjectProperties(Item, Variables);

				if (this.columns == null)
				{
					Type ItemType = Item.GetType();
					IEnumerable<PropertyInfo> ItemProperties = ItemType.GetRuntimeProperties();
					IEnumerable<FieldInfo> ItemFields = ItemType.GetRuntimeFields();

					if (ItemType != LastItemType)
					{
						foreach (PropertyInfo Property in ItemProperties)
						{
							if (!Columns.ContainsKey(Property.Name))
								Columns[Property.Name] = c++;
						}

						foreach (FieldInfo Field in ItemFields)
						{
							if (!Columns.ContainsKey(Field.Name))
								Columns[Field.Name] = c++;
						}

						LastItemType = ItemType;
					}

					Rec = new IElement[c];

					foreach (PropertyInfo Property in ItemProperties)
					{
						i = Columns[Property.Name];
						Rec[i] = Expression.Encapsulate(Property.GetValue(Item));
					}

					foreach (FieldInfo Field in ItemFields)
					{
						i = Columns[Field.Name];
						Rec[i] = Expression.Encapsulate(Field.GetValue(Item));
					}
				}
				else
				{
					Rec = new IElement[c];

					for (i = 0; i < c; i++)
					{
						try
						{
							Rec[i] = this.columns[i].Evaluate(Properties);
						}
						catch (Exception ex)
						{
							Rec[i] = Expression.Encapsulate(ex);
						}
					}
				}

				Items.AddLast(Rec);
				NrRecords++;

				if (ManualTop && NrRecords >= Top)
					break;
			}

			IElement[] Elements = new IElement[NrRecords * c];

			i = 0;
			foreach (object[] Record in Items)
			{
				foreach (IElement Item in Record)
					Elements[i++] = Item;

				while (i % c != 0)
					Elements[i++] = new ObjectValue(null);
			}

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
						if (E == null)
						{
							if (Names[i] == null)
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
				if (Names[i] == null)
					Names[i] = (i + 1).ToString();

				i++;
			}

			Result.ColumnNames = Names;

			return Result;

			// TODO: Joins
			// TODO: Source names
		}

		private static MethodInfo findMethod = null;

		private static Filter Convert(ScriptNode Conditions, Variables Variables)
		{
			if (Conditions == null)
				return null;
			else if (Conditions is Operators.Logical.And And)
			{
				Filter L = Convert(And.LeftOperand, Variables);
				Filter R = Convert(And.RightOperand, Variables);

				if (L == null && R == null)
					return null;
				else if (L == null)
					return R;
				else if (R == null)
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
			else if (Conditions is Operators.Logical.Or Or)
			{
				Filter L = Convert(Or.LeftOperand, Variables);
				Filter R = Convert(Or.RightOperand, Variables);

				if (L == null || R == null)
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
			else if (Conditions is Operators.Logical.Not Not)
			{
				Filter F = Convert(Not.Operand, Variables);
				if (F == null)
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

	}
}
