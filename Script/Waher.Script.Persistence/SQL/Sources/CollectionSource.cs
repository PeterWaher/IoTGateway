using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data Source defined by a collection name
	/// </summary>
	public class CollectionSource : IDataSource
	{
		private readonly string collectionName;

		/// <summary>
		/// Data Source defined by a collection name
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		public CollectionSource(string CollectionName)
		{
			this.collectionName = CollectionName;
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
		public IEnumerator Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			return Find(this.collectionName, Offset, Top, Where, Variables, Order, Node);
		}

		internal static IEnumerator Find(string Collection, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			object[] FindParameters = new object[] { Collection, Offset, Top, Convert(Where, Variables), Convert(Order) };
			object Obj = FindMethod.Invoke(null, FindParameters);
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
		/// Generic object database Find method: <see cref="Database.Find(string, int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo FindMethod
		{
			get
			{
				if (findMethod is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("Find"))
					{
						if (MI.ContainsGenericParameters)
							continue;

						ParameterInfo[] Parameters = MI.GetParameters();
						if (Parameters.Length != 5 ||
							Parameters[0].ParameterType != typeof(string) ||
							Parameters[1].ParameterType != typeof(int) ||
							Parameters[2].ParameterType != typeof(int) ||
							Parameters[3].ParameterType != typeof(Filter) ||
							Parameters[4].ParameterType != typeof(string[]))
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

		internal static string[] Convert(KeyValuePair<VariableReference, bool>[] Order)
		{
			if (Order is null)
				return null;

			int i, c = Order.Length;
			string[] Result = new string[c];

			for (i = 0; i < c; i++)
			{
				if (Order[i].Value)
					Result[i] = Order[i].Key.VariableName;
				else
					Result[i] = "-" + Order[i].Key.VariableName;
			}

			return Result;
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
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		public void Update(IEnumerable<object> Objects)
		{
			Task _ = Database.Update(Objects);
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public void Delete(IEnumerable<object> Objects)
		{
			Task _ = Database.Delete(Objects);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public void Insert(object Object)
		{
			Task _ = Database.Insert(Object);
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get { return this.collectionName; }
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get { return string.Empty; }
		}

	}
}
