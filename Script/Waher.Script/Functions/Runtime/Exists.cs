using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Membership;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Checks if an expression exists, or has a valid value.
	/// </summary>
	public class Exists : FunctionOneVariable
	{
		private enum ExecutionMode
		{
			Default,
			Variable,
			VectorIndex,
			NamedProperty
		};

		private readonly ScriptNode vectorIndex;
		private readonly string variableName;
		private readonly string memberName;
		private readonly ExecutionMode mode;

		/// <summary>
		/// Checks if an expression exists, or has a valid value.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Exists(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
			this.mode = ExecutionMode.Default;

			if (Argument is VariableReference Ref)
			{
				this.variableName = Ref.VariableName;
				this.mode = ExecutionMode.Variable;
			}
			else if (Argument is VectorIndex VectorIndex)
			{
				if (VectorIndex.LeftOperand is VariableReference VRef)
				{
					this.variableName = VRef.VariableName;
					this.vectorIndex = VectorIndex.RightOperand;
					this.mode = ExecutionMode.VectorIndex;
				}
			}
			else if (Argument is NamedMember NamedMember)
			{
				if (NamedMember.Operand is VariableReference VRef)
				{
					this.variableName = VRef.VariableName;
					this.memberName = NamedMember.Name;
					this.mode = ExecutionMode.NamedProperty;
				}
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Exists);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Checks if an element is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <returns>If element is well-defined.</returns>
		public bool IsWellDefined(IElement Element)
		{
			return this.IsWellDefined(Element?.AssociatedObjectValue);
		}

		/// <summary>
		/// Checks if an object is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Obj">Element</param>
		/// <returns>If element is well-defined.</returns>
		public bool IsWellDefined(object Obj)
		{
			if (Obj is null)
				return false;

			if (Obj is double d)
				return !double.IsNaN(d);

			if (Obj is Complex z)
				return !double.IsNaN(z.Real) && !double.IsNaN(z.Imaginary);

			return true;
		}

		/// <summary>
		/// Checks if an element index is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <param name="Index">Index into element.</param>
		/// <param name="Node">Node executing the method.</param>
		/// <returns>If element index is well-defined.</returns>
		public async Task<bool> IsWellDefined(IElement Element, IElement Index, ScriptNode Node)
		{
			if (!this.IsWellDefined(Element) || !this.IsWellDefined(Index))
				return false;

			object Obj = Element.AssociatedObjectValue;
			object IndexObj = Index.AssociatedObjectValue;

			if (IndexObj is string StringIndex)
				return await this.IsWellDefined(Element, StringIndex, Node);

			IElement Value = null;

			if (IndexObj is double d)
			{
				if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
					return false;
			}
			else
			{
				try
				{
					Value = await VectorIndex.EvaluateIndex(Element, Index, false, this);
					return this.IsWellDefined(Value);
				}
				catch (Exception)
				{
					return false;
				}
			}

			int i = (int)d;

			if (Element is IVector V)
			{
				if (i >= V.Dimension)
					return false;

				Value = V.GetElement(i);
			}
			else if (Element is Abstraction.Sets.ISet S)
			{
				if (S.Size.HasValue)
				{
					if (i >= S.Size.Value)
						return false;
				}

				try
				{
					foreach (IElement E in S.ChildElements)
					{
						if (i-- == 0)
						{
							Value = E;
							break;
						}
					}
				}
				catch (Exception)
				{
					return false;
				}
			}
			else if (Element is StringValue s)
			{
				if (s.Value is null)
					return false;

				return i < s.Value.Length;
			}
			else
			{
				Type T = Obj.GetType();

				if (VectorIndex.TryGetIndexProperty(T, true, false, out PropertyInfo ItemProperty,
					out ParameterInfo[] Parameters) &&
					(Parameters?.Length ?? 0) == 1)
				{
					try
					{
						Value = await VectorIndex.EvaluateIndex(Obj, T, ItemProperty, Parameters, Index, Node);
					}
					catch (Exception)
					{
						return false;
					}
				}
				else
				{
					try
					{
						Value = await VectorIndex.EvaluateIndex(Element, Index, false, this);
					}
					catch (Exception)
					{
						return false;
					}
				}
			}

			return this.IsWellDefined(Value);
		}

		/// <summary>
		/// Checks if an element property is well-defined, i.e., not null or NaN, etc.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <param name="MemberName">Named member.</param>
		/// <param name="Node">Node executing the method.</param>
		/// <returns>If element property is well-defined.</returns>
		public async Task<bool> IsWellDefined(IElement Element, string MemberName, ScriptNode Node)
		{
			if (!this.IsWellDefined(Element) || string.IsNullOrEmpty(MemberName))
				return false;

			object Object = Element.AssociatedObjectValue;
			object PropertyValue;

			if (Object is IDictionary<string, IElement> Obj)
			{
				if (Obj.TryGetValue(MemberName, out IElement E))
					PropertyValue = E.AssociatedObjectValue;
				else
					return false;
			}
			else if (Object is IDictionary<string, object> Obj2)
			{
				if (!Obj2.TryGetValue(MemberName, out PropertyValue))
					return false;
			}
			else
			{
				Type T = Object.GetType();
				PropertyInfo Property = T.GetRuntimeProperty(MemberName);
				if (!(Property is null))
				{
					if (!Property.CanRead || !Property.GetMethod.IsPublic)
						return false;
					else
						PropertyValue = await WaitPossibleTask(Property.GetValue(Object, null));
				}
				else
				{
					FieldInfo Field = T.GetRuntimeField(MemberName);
					if (!(Field is null))
					{
						if (!Field.IsPublic)
							return false;
						else
							PropertyValue = Field.GetValue(Object);
					}
					else
					{
						EventInfo Event = T.GetRuntimeEvent(MemberName);
						if (!(Event is null))
							return true;
						else
						{
							foreach (MethodInfo MI in T.GetRuntimeMethods())
							{
								if (!MI.IsAbstract && MI.IsPublic && MI.Name == MemberName)
									return true;
							}
						}

						if (VectorIndex.TryGetIndexProperty(T, true, false, out Property,
							out ParameterInfo[] IndexArguments) &&
							(IndexArguments?.Length ?? 0) == 1)
						{
							object[] Index;

							if (IndexArguments[0].ParameterType == typeof(string))
								Index = new object[] { MemberName };
							else
								Index = new object[] { Expression.ConvertTo(MemberName, IndexArguments[0].ParameterType, Node) };

							PropertyValue = await WaitPossibleTask(Property.GetValue(Object, Index));
						}
						else
							return false;
					}
				}
			}

			return this.IsWellDefined(PropertyValue);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			try
			{
				IElement E;

				if (this.mode == ExecutionMode.Default)
				{
					E = await this.Argument.EvaluateAsync(Variables);
					if (!this.IsWellDefined(E))
						return BooleanValue.False;
				}
				else
				{
					if (Variables.TryGetVariable(this.variableName, out Variable v))
						E = v.ValueElement;
					else if (!Expression.TryGetConstant(this.variableName, Variables, out E))
					{
						if (Types.IsRootNamespace(this.variableName))
						{
							if (this.mode == ExecutionMode.Variable)
								return BooleanValue.True;
							else
								E = new Namespace(this.variableName);
						}
						else
						{
							ILambdaExpression Lambda = Expression.GetFunctionLambdaDefinition(this.variableName, this.Start, this.Length, this.Expression);
							if (Lambda is null)
								return BooleanValue.False;

							if (this.mode == ExecutionMode.Variable)
								return BooleanValue.True;
							else
								E = new ObjectValue(Lambda);
						}
					}

					if (!this.IsWellDefined(E))
						return BooleanValue.False;

					switch (this.mode)
					{
						case ExecutionMode.VectorIndex:
							IElement Index = await this.vectorIndex.EvaluateAsync(Variables);
							if (!await this.IsWellDefined(E, Index, this))
								return BooleanValue.False;
							break;

						case ExecutionMode.NamedProperty:
							if (!await this.IsWellDefined(E, this.memberName, this))
								return BooleanValue.False;
							break;
					}
				}

				return BooleanValue.True;
			}
			catch (Exception)
			{
				return BooleanValue.False;
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return BooleanValue.True;
		}
	}
}
