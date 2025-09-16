﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Enumerators;

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
		/// <param name="Generic">If objects of type <see cref="GenericObject"/> should be returned.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public async Task<IResultSetEnumerator> Find(int Offset, int Top, bool Generic, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			object[] FindParameters;
			MethodInfo MI;

			if (Generic)
			{
				FindParameters = new object[] { await Database.GetCollection(this.type), Offset, Top, await ConvertAsync(Where, Variables, this.Name), Convert(Order) };
				MI = FindMethodGeneric.MakeGenericMethod(typeof(GenericObject));
			}
			else
			{
				FindParameters = new object[] { Offset, Top, await ConvertAsync(Where, Variables, this.Name), Convert(Order) };
				MI = FindMethod.MakeGenericMethod(this.type);
			}

			object Obj = MI.Invoke(null, FindParameters);
			Obj = await ScriptNode.WaitPossibleTask(Obj);

			if (!(Obj is IEnumerable Enumerable))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			return new SynchEnumerator(Enumerable.GetEnumerator());
		}

		private static MethodInfo findMethod = null;
		private static MethodInfo findMethodGeneric = null;

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

		/// <summary>
		/// Generic object database Find method: <see cref="Database.Find{T}(string, int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo FindMethodGeneric
		{
			get
			{
				if (findMethodGeneric is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("Find"))
					{
						if (!MI.ContainsGenericParameters)
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

						findMethodGeneric = MI;
					}

					if (findMethodGeneric is null)
						throw new InvalidOperationException("Appropriate Database.Find method not found.");
				}

				return findMethodGeneric;
			}
		}

		/// <summary>
		/// Finds and Deletes a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Number of objects deleted, if known.</returns>
		public async Task<int?> FindDelete(bool Lazy, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			Filter Filter = await TypeSource.ConvertAsync(Where, Variables, this.Name);

			object[] FindParameters = new object[] { Offset, Top, Filter, Convert(Order) };
			object Obj = (Lazy ? DeleteLazyMethod : FindDeleteMethod).MakeGenericMethod(this.type).Invoke(null, FindParameters);
			
			if (Lazy)
				return null;

			Obj = await ScriptNode.WaitPossibleTask(Obj);
			if (!(Obj is IEnumerable<object> Objects))
				throw new ScriptRuntimeException("Unexpected response.", Node);

			int Count = 0;

			foreach (object Obj2 in Objects)
				Count++;

			return Count;
		}

		private static MethodInfo findDeleteMethod = null;
		private static MethodInfo deleteLazyMethod = null;

		/// <summary>
		/// Generic object database FindDelete method: <see cref="Database.FindDelete{T}(int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo FindDeleteMethod
		{
			get
			{
				if (findDeleteMethod is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("FindDelete"))
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

						findDeleteMethod = MI;
					}

					if (findDeleteMethod is null)
						throw new InvalidOperationException("Appropriate Database.FindDelete method not found.");
				}

				return findDeleteMethod;
			}
		}

		/// <summary>
		/// Generic object database DeleteLazy method: <see cref="Database.DeleteLazy{T}(int, int, Filter, string[])"/>
		/// </summary>
		public static MethodInfo DeleteLazyMethod
		{
			get
			{
				if (deleteLazyMethod is null)
				{
					foreach (MethodInfo MI in typeof(Database).GetTypeInfo().GetDeclaredMethods("DeleteLazy"))
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

						deleteLazyMethod = MI;
					}

					if (deleteLazyMethod is null)
						throw new InvalidOperationException("Appropriate Database.DeleteLazy method not found.");
				}

				return deleteLazyMethod;
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

		internal static async Task<Filter> ConvertAsync(ScriptNode Conditions, Variables Variables, string Name)
		{
			if (Conditions is null)
				return null;

			try
			{
				if (Conditions is TernaryOperator Tercery)
				{
					if (Conditions is Operators.Comparisons.Range Range &&
						Range.MiddleOperand is VariableReference Ref)
					{
						ScriptNode LO = Reduce(Range.LeftOperand, Name);
						ScriptNode RO = Reduce(Range.RightOperand, Name);
						string FieldName = Ref.VariableName;
						object Min = (await LO.EvaluateAsync(Variables))?.AssociatedObjectValue ?? null;
						object Max = (await RO.EvaluateAsync(Variables))?.AssociatedObjectValue ?? null;

						Filter[] Filters = new Filter[2];

						if (Range.LeftInclusive)
							Filters[0] = new FilterFieldGreaterOrEqualTo(Ref.VariableName, Min);
						else
							Filters[0] = new FilterFieldGreaterThan(Ref.VariableName, Min);

						if (Range.RightInclusive)
							Filters[1] = new FilterFieldLesserOrEqualTo(Ref.VariableName, Max);
						else
							Filters[1] = new FilterFieldLesserThan(Ref.VariableName, Max);

						return new FilterAnd(Filters);
					}
				}
				else if (Conditions is BinaryOperator Bin)
				{
					ScriptNode LO = Reduce(Bin.LeftOperand, Name);
					ScriptNode RO = Reduce(Bin.RightOperand, Name);

					if (Conditions is Operators.Logical.And || Conditions is Operators.Dual.And)
					{
						Filter L = await ConvertAsync(LO, Variables, Name);
						Filter R = await ConvertAsync(RO, Variables, Name);

						ChunkedList<Filter> Filters = new ChunkedList<Filter>();

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

					if (Conditions is Operators.Logical.Or || Conditions is Operators.Dual.Or)
					{
						Filter L = await ConvertAsync(LO, Variables, Name);
						Filter R = await ConvertAsync(RO, Variables, Name);

						ChunkedList<Filter> Filters = new ChunkedList<Filter>();

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

					if (LO is VariableReference LVar)
					{
						string FieldName = LVar.VariableName;
						object Value = (await RO.EvaluateAsync(Variables))?.AssociatedObjectValue ?? null;

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
							string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
							Like.TransformExpression += (Expression) => Database.WildcardToRegex(Expression, "%");
							return new FilterFieldLikeRegEx(FieldName, RegEx);
						}
						else if (Conditions is Operators.Comparisons.NotLike NotLike)
						{
							string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
							NotLike.TransformExpression += (Expression) => Database.WildcardToRegex(Expression, "%");
							return new FilterNot(new FilterFieldLikeRegEx(FieldName, RegEx));
						}
					}
					else if (RO is VariableReference RVar)
					{
						string FieldName = RVar.VariableName;
						object Value = (await LO.EvaluateAsync(Variables))?.AssociatedObjectValue ?? null;

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
							string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
							Like.TransformExpression += (Expression) => Database.WildcardToRegex(Expression, "%");
							return new FilterFieldLikeRegEx(FieldName, RegEx);
						}
						else if (Conditions is Operators.Comparisons.NotLike NotLike)
						{
							string RegEx = Database.WildcardToRegex(Value is string s ? s : Expression.ToString(Value), "%");
							NotLike.TransformExpression += (Expression) => Database.WildcardToRegex(Expression, "%");
							return new FilterNot(new FilterFieldLikeRegEx(FieldName, RegEx));
						}
					}
				}
				else if (Conditions is UnaryOperator UnOp)
				{
					if (Conditions is Operators.Logical.Not Not)
					{
						Filter F = await ConvertAsync(Reduce(Not.Operand, Name), Variables, Name);
						if (F is FilterNot Not2)
							return Not2.ChildFilter;
						else
							return new FilterNot(F);
					}
				}
				else if (Conditions is VariableReference Ref)
					return new FilterFieldEqualTo(Ref.VariableName, true);

				return new FilterCustom<object>(new ScriptNodeFilter(Conditions, Variables).Passes);
			}
			catch (Exception)
			{
				return new FilterCustom<object>(new ScriptNodeFilter(Conditions, Variables).Passes);
			}
		}

		private class ScriptNodeFilter
		{
			private readonly ScriptNode node;
			private readonly Variables variables;
			private ObjectProperties properties = null;

			public ScriptNodeFilter(ScriptNode Node, Variables Variables)
			{
				this.node = Node;
				this.variables = Variables;
				this.properties = null;
			}

			public bool Passes(object Object)
			{
				if (this.properties is null)
					this.properties = new ObjectProperties(Object, this.variables, true);
				else
					this.properties.Object = Object;

				try
				{
					IElement Result = this.node.Evaluate(this.properties);
					return Result.AssociatedObjectValue is bool bv && bv;
				}
				catch (Exception)
				{
					return false;
				}
			}
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
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Objects">Objects to update</param>
		public Task Update(bool Lazy, IEnumerable<object> Objects)
		{
			return Lazy ? Database.UpdateLazy(Objects) : Database.Update(Objects);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(bool Lazy, object Object)
		{
			return Lazy ? Database.InsertLazy(Object) : Database.Insert(Object);
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
					PropertyInfo PI = this.type.GetRuntimeProperty(Label);

					if (PI is null)
					{
						FieldInfo FI = this.type.GetRuntimeField(Label);
						Result = FI?.IsPublic ?? false;
					}
					else
						Result = PI.CanRead && PI.GetMethod.IsPublic;

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
		/// Drops the collection specified by the source.
		/// </summary>
		public Task DropCollection()
		{
			throw InvalidOperation();
		}

	}
}
