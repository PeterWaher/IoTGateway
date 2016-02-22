using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
    /// <summary>
    /// As operator
    /// </summary>
    public class As : BinaryScalarOperator
    {
        /// <summary>
        /// As operator.
        /// </summary>
        /// <param name="Left">Left operand.</param>
        /// <param name="Right">Right operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public As(ScriptNode Left, ScriptNode Right, int Start, int Length)
            : base(Left, Right, Start, Length)
        {
        }

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(IElement Left, IElement Right)
        {
            TypeValue TypeValue;
            if ((TypeValue = Right as TypeValue) != null)
            {
                Type T = TypeValue.Value;
                object Obj = Left.AssociatedObjectValue;

                if (T.IsInstanceOfType(Obj))
                    return Left;
                else 
                    return ObjectValue.Null;
            }
            else
                return base.Evaluate(Left, Right);
        }

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <returns>Result</returns>
        public override IElement EvaluateScalar(IElement Left, IElement Right)
        {
            TypeValue TypeValue;
            if ((TypeValue = Right as TypeValue) != null)
            {
                Type T = TypeValue.Value;
                object Obj = Left.AssociatedObjectValue;

                if (T.IsInstanceOfType(Obj))
                    return Left;
                else
                    return ObjectValue.Null;
            }
            else
                throw new ScriptRuntimeException("Right operand in an AS operation must be a type value.", this);
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
