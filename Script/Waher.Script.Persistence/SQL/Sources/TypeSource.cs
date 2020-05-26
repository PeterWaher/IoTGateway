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
	/// Data Source defined by a type definition
	/// </summary>
	public class TypeSource : IDataSource
	{
		private readonly Dictionary<string, bool> isLabel = new Dictionary<string, bool>();
		private readonly Type type;
		private readonly string alias;

		/// <summary>
		/// Data Source defined by a type definition
		/// </summary>
		/// <param name="Type">Type definition</param>
		/// <param name="Alias">Optional alias for source.</param>
		public TypeSource(Type Type, string Alias)
		{
			this.type = Type;
			this.alias = Alias;
		}

		/// <summary>
		/// Type definition
		/// </summary>
		public Type Type => this.type;

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
		public Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			return Find(this.type, Offset, Top, Where, Variables, Order, Node, this.Name);
		}

		internal static async Task<IResultSetEnumerator> Find(Type T, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node, string Name)
		{
			bool Complete = true;
			object[] FindParameters = new object[] { Offset, Top, Convert(Where, Variables, Name, ref Complete), Convert(Order) };
			object Obj = FindMethod.MakeGenericMethod(T).Invoke(null, FindParameters);
			if (!(Obj is Task Task))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			await Task;

			PropertyInfo PI = Task.GetType().GetRuntimeProperty("Result");
			if (PI is null)
				throw new ScriptRuntimeException("Unexpected response.", Node);

			Obj = PI.GetValue(Task);
			if (!(Obj is IEnumerable Enumerable))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			IResultSetEnumerator Result = new SynchEnumerator(Enumerable.GetEnumerator());

			if (!Complete)
				Result = new ConditionalEnumerator(Result, Variables, Where);

			return Result;
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

		internal static Filter Convert(ScriptNode Conditions, Variables Variables, string Name, ref bool Complete)
		{
			if (Conditions is null)
				return null;

			if (Conditions is BinaryOperator Bin)
			{
				ScriptNode LO = Reduce(Bin.LeftOperand, Name);
				ScriptNode RO = Reduce(Bin.RightOperand, Name);

				Operators.Logical.And And = Conditions as Operators.Logical.And;
				Operators.Dual.And And2 = And is null ? Conditions as Operators.Dual.And : null;

				if (And != null || And2 != null)
				{
					Filter L = Convert(LO, Variables, Name, ref Complete);
					Filter R = Convert(RO, Variables, Name, ref Complete);

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
					Filter L = Convert(LO, Variables, Name, ref Complete);
					Filter R = Convert(RO, Variables, Name, ref Complete);

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

				if (LO is VariableReference LVar)
				{
					string FieldName = LVar.VariableName;
					object Value = RO.Evaluate(Variables)?.AssociatedObjectValue ?? null;

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
					else
						Complete = false;
				}
				else if (RO is VariableReference RVar)
				{
					string FieldName = RVar.VariableName;
					object Value = LO.Evaluate(Variables)?.AssociatedObjectValue ?? null;

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
						return new FilterFieldLesserThan(FieldName, Value);
					else if (Conditions is Operators.Comparisons.GreaterThanOrEqualTo)
						return new FilterFieldLesserOrEqualTo(FieldName, Value);
					else if (Conditions is Operators.Comparisons.LesserThan)
						return new FilterFieldGreaterThan(FieldName, Value);
					else if (Conditions is Operators.Comparisons.LesserThanOrEqualTo)
						return new FilterFieldGreaterOrEqualTo(FieldName, Value);
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
					else
						Complete = false;
				}
				else
					Complete = false;
			}
			else if (Conditions is UnaryOperator UnOp)
			{
				if (Conditions is Operators.Logical.Not Not)
				{
					Filter F = Convert(Reduce(Not.Operand, Name), Variables, Name, ref Complete);
					if (F is null)
						return null;
					else if (F is FilterNot Not2)
						return Not2.ChildFilter;
					else
						return new FilterNot(F);
				}
				else
					Complete = false;
			}
			else if (Conditions is VariableReference Ref)
				return new FilterFieldEqualTo(Ref.VariableName, true);
			else
				Complete = false;

			return null;
		}

		private static ScriptNode Reduce(ScriptNode Node, string Name)
		{
			if (Node is Operators.Membership.NamedMember N &&
				N.Operand is VariableReference Ref &&
				Ref.VariableName == Name)
			{
				return new VariableReference(N.Name, 0, 0, Node.Expression);
			}
			else
				return Node;
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
		public Task Update(IEnumerable<object> Objects)
		{
			return Database.Update(Objects);
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			return Database.Delete(Objects);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(object Object)
		{
			return Database.Insert(Object);
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get { return Database.GetCollection(this.type).Result; }
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get { return this.type.FullName; }
		}

		/// <summary>
		/// Collection name or alias.
		/// </summary>
		public string Name
		{
			get => string.IsNullOrEmpty(this.alias) ? this.type.Name : this.alias;
		}

		/// <summary>
		/// Checks if the name refers to the source.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name refers to the source.</returns>
		public bool IsSource(string Name)
		{
			return
				string.Compare(this.type.Name, Name, true) == 0 ||
				string.Compare(this.alias, Name, true) == 0;
		}

		/// <summary>
		/// Checks if the label is a label in the source.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>If the label is a label in the source.</returns>
		public Task<bool> IsLabel(string Label)
		{
			lock (this.isLabel)
			{
				if (!this.isLabel.TryGetValue(Label, out bool Result))
				{
					Result = !(this.type.GetRuntimeProperty(Label) is null &&
						this.type.GetRuntimeField(Label) is null);

					this.isLabel[Label] = Result;
				}

				return Task.FromResult<bool>(Result);
			}
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

		private static Exception InvalidOperation()
		{
			throw new InvalidOperationException("Indices and collections for types are defined in the class definition, not by script.");
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
