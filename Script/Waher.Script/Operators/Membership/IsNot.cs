using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Is Not operator
	/// </summary>
	public class IsNot : BinaryScalarOperator
    {
		/// <summary>
		/// Is Not operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IsNot(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
            if (Right is TypeValue TypeValue)
                return this.EvaluateIsNot(Left, TypeValue);
            else
                return base.Evaluate(Left, Right, Variables);
        }

        private BooleanValue EvaluateIsNot(IElement Left, TypeValue Right)
		{
            Type Expected = Right.Value;
            object Obj = Left.AssociatedObjectValue;
            Type Candidate = Obj?.GetType() ?? typeof(object);

            if (Expected.GetTypeInfo().IsAssignableFrom(Candidate.GetTypeInfo()))
                return BooleanValue.False;
            else
                return BooleanValue.True;
        }

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override Task<IElement> EvaluateAsync(IElement Left, IElement Right, Variables Variables)
        {
            if (Right is TypeValue TypeValue)
                return Task.FromResult<IElement>(this.EvaluateIsNot(Left, TypeValue));
            else
                return base.EvaluateAsync(Left, Right, Variables);
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
            if (Right is TypeValue TypeValue)
                return this.EvaluateIsNot(Left, TypeValue);
            else
                throw new ScriptRuntimeException("Right operand in an IS NOT operation must be a type value.", this);
        }

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override Task<IElement> EvaluateScalarAsync(IElement Left, IElement Right, Variables Variables)
        {
            if (Right is TypeValue TypeValue)
                return Task.FromResult<IElement>(this.EvaluateIsNot(Left, TypeValue));
            else
                throw new ScriptRuntimeException("Right operand in an IS NOT operation must be a type value.", this);
        }

        /// <summary>
        /// How scalar operands of different types are to be treated. By default, scalar operands are required to be of the same type.
        /// </summary>
        public override UpgradeBehaviour ScalarUpgradeBehaviour => UpgradeBehaviour.DifferentTypesOk;
    }
}
