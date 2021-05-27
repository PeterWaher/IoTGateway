using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Element-wise Addition operator.
	/// </summary>
	public class AddElementWise : BinaryElementWiseOperator, IDifferentiable
	{
		/// <summary>
		/// Element-wise Addition operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public AddElementWise(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
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
			if (Left is DoubleNumber DL && Right is DoubleNumber DR)
				return new DoubleNumber(DL.Value + DR.Value);

			if (Left is ISemiGroupElementWise LE && Right is ISemiGroupElementWise RE)
			{
				ISemiGroupElementWise Result = LE.AddRightElementWise(RE);
				if (!(Result is null))
					return Result;

				Result = RE.AddLeftElementWise(LE);
				if (!(Result is null))
					return Result;
			}

			return Add.EvaluateAddition(Left, Right, this);
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (this.left is IDifferentiable Left &&
				this.right is IDifferentiable Right)
			{
				return new AddElementWise(
					Left.Differentiate(VariableName, Variables),
					Right.Differentiate(VariableName, Variables),
					this.Start, this.Length, this.Expression);
			}
			else
				throw new ScriptRuntimeException("Terms not differentiable.", this);
		}

	}
}
