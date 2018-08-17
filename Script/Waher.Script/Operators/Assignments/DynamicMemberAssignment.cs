using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Membership;

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
			IElement Middle = this.middle.Evaluate(Variables);
			if (!(Middle is StringValue S))
				throw new ScriptRuntimeException("Member names must be strings.", this);

			string Name = S.Value;

			IElement Left = this.left.Evaluate(Variables);
			IElement Right = this.right.Evaluate(Variables);
			object LeftValue = Left.AssociatedObjectValue;
			Type Type = LeftValue.GetType();

			PropertyInfo Property = Type.GetRuntimeProperty(Name);
			if (Property != null)
			{
				Type = Property.PropertyType;
				if (!Type.GetTypeInfo().IsAssignableFrom(Right.GetType().GetTypeInfo()))
					Property.SetValue(LeftValue, Expression.ConvertTo(Right, Type, this), null);
				else
					Property.SetValue(LeftValue, Right, null);
			}
			else
			{
				FieldInfo Field = Type.GetRuntimeField(Name);
				if (Field != null)
				{
					Type = Field.FieldType;
					if (!Type.GetTypeInfo().IsAssignableFrom(Right.GetType().GetTypeInfo()))
						Field.SetValue(Left, Expression.ConvertTo(Right, Type, this));
					else
						Field.SetValue(Left, Right);
				}
				else
				{
					Property = Type.GetRuntimeProperty("Item");
					if (Property != null)
					{
						Type = Property.PropertyType;
						if (!Type.GetTypeInfo().IsAssignableFrom(Right.GetType().GetTypeInfo()))
							Property.SetValue(LeftValue, Expression.ConvertTo(Right, Type, this), new string[] { Name });
						else
							Property.SetValue(LeftValue, Right, new string[] { Name });
					}
					else
						throw new ScriptRuntimeException("Member '" + Name + "' not found on type '" + Type.FullName + "'.", this);
				}
			}

			return Right;
		}

	}
}
