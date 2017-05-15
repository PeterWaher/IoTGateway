using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Is operator
	/// </summary>
	public class Is : BinaryScalarOperator
    {
		/// <summary>
		/// Is operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Is(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(IElement Left, IElement Right, Variables Variables)
        {
            TypeValue TypeValue;
            if ((TypeValue = Right as TypeValue) != null)
            {
                Type T = TypeValue.Value;
                object Obj = Left.AssociatedObjectValue;

				if (Obj != null && T.GetTypeInfo().IsAssignableFrom(Obj.GetType().GetTypeInfo()))
					return BooleanValue.True;
                else
                    return BooleanValue.False;
            }
            else
                return base.Evaluate(Left, Right, Variables);
        }

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override IElement EvaluateScalar(IElement Left, IElement Right, Variables Variables)
        {
            TypeValue TypeValue;
            if ((TypeValue = Right as TypeValue) != null)
            {
                Type T = TypeValue.Value;
                object Obj = Left.AssociatedObjectValue;

				if (Obj != null && T.GetTypeInfo().IsAssignableFrom(Obj.GetType().GetTypeInfo()))
					return BooleanValue.True;
                else
                    return BooleanValue.False;
            }
            else
                throw new ScriptRuntimeException("Right operand in an IS operation must be a type value.", this);
        }

        /// <summary>
        /// How scalar operands of different types are to be treated. By default, scalar operands are required to be of the same type.
        /// </summary>
        public override UpgradeBehaviour ScalarUpgradeBehaviour
        {
            get { return UpgradeBehaviour.DifferentTypesOk; }
        }
    }
}
