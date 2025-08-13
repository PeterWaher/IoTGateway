using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Membership;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Dynamic member Assignment operator.
	/// </summary>
	public class DynamicMemberAssignment : TernaryOperator
	{
		/// <summary>
		/// Dynamic member Assignment operator.
		/// </summary>
		/// <param name="DynamicMember">Dynamic member</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DynamicMemberAssignment(DynamicMember DynamicMember, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(DynamicMember.LeftOperand, DynamicMember.RightOperand, Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			IElement Middle = this.middle.Evaluate(Variables);
			IElement Right = this.right.Evaluate(Variables);

			if (Middle.IsScalar)
				return this.Evaluate(Left, Middle, Right);
			else
			{
				foreach (IElement MiddleElement in Middle.ChildElements)
					this.Evaluate(Left, MiddleElement, Right);

				return Right;
			}
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement Left = await this.left.EvaluateAsync(Variables);
			IElement Middle = await this.middle.EvaluateAsync(Variables);
			IElement Right = await this.right.EvaluateAsync(Variables);

			if (Middle.IsScalar)
				return this.Evaluate(Left, Middle, Right);
			else
			{
				foreach (IElement MiddleElement in Middle.ChildElements)
					this.Evaluate(Left, MiddleElement, Right);

				return Right;
			}
		}

		/// <summary>
		/// Performs scalar dynamic member assignment.
		/// </summary>
		/// <param name="Left">Object</param>
		/// <param name="Middle">Member</param>
		/// <param name="Right">Value to assign.</param>
		/// <returns>Result</returns>
		public IElement Evaluate(IElement Left, IElement Middle, IElement Right)
		{
			if (!(Middle.AssociatedObjectValue is string Name))
				throw new ScriptRuntimeException("Member names must be strings.", this);

			object LeftValue = Left.AssociatedObjectValue;
			Type Type = LeftValue.GetType();

			PropertyInfo Property = Type.GetRuntimeProperty(Name);
			if (!(Property is null))
			{
				if (!Property.CanWrite)
					throw new ScriptRuntimeException("Property cannot be set: " + Name, this);
				else if (!Property.SetMethod.IsPublic)
					throw new ScriptRuntimeException("Property not accessible: " + Name, this);
				else
				{
					Type = Property.PropertyType;
					if (!Type.IsAssignableFrom(Right.GetType().GetTypeInfo()))
						Property.SetValue(LeftValue, Expression.ConvertTo(Right, Type, this), null);
					else
						Property.SetValue(LeftValue, Right, null);
				}
			}
			else
			{
				FieldInfo Field = Type.GetRuntimeField(Name);
				if (!(Field is null))
				{
					if (!Field.IsPublic)
						throw new ScriptRuntimeException("Field not accessible: " + Name, this);
					else
					{
						Type = Field.FieldType;
						if (!Type.IsAssignableFrom(Right.GetType().GetTypeInfo()))
							Field.SetValue(Left, Expression.ConvertTo(Right, Type, this));
						else
							Field.SetValue(Left, Right);
					}
				}
				else
				{
					if (VectorIndex.TryGetIndexProperty(Type, false, true, out Property, 
						out ParameterInfo[] IndexArguments) &&
						(IndexArguments?.Length ?? 0) == 1)
					{
						object[] Index;

						if (IndexArguments[0].ParameterType == typeof(string))
							Index = new object[] { Name };
						else
							Index = new object[] { Expression.ConvertTo(Name, IndexArguments[0].ParameterType, this) };

						Type = Property.PropertyType;
						if (Type == typeof(object))
							Property.SetValue(LeftValue, Right.AssociatedObjectValue, Index);
						else if (Type.IsAssignableFrom(Right.GetType().GetTypeInfo()))
							Property.SetValue(LeftValue, Right, Index);
						else
							Property.SetValue(LeftValue, Expression.ConvertTo(Right, Type, this), Index);
					}
					else
						throw new ScriptRuntimeException("Member '" + Name + "' not found on type '" + Type.FullName + "'.", this);
				}
			}

			return Right;
		}

	}
}
