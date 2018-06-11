using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Analytic;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Element-wise Power operator.
	/// </summary>
	public class PowerElementWise : BinaryElementWiseDoubleOperator, IDifferentiable
	{
		/// <summary>
		/// Element-wise Power operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PowerElementWise(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			return Power.EvaluatePower(Left, Right, this);
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Left, double Right)
		{
			return new DoubleNumber(Math.Pow(Left, Right));
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
				int Start = this.Start;
				int Len = this.Length;
				Expression Expression = this.Expression;

				return new MultiplyElementWise(
					this,
					new Add(
						new MultiplyElementWise(
							Left.Differentiate(VariableName, Variables),
							new Divide(this.right, this.left, Start, Len, Expression),
							Start, Len, Expression),
						new MultiplyElementWise(
							Right.Differentiate(VariableName, Variables),
							new Ln(this.left, Start, Len, Expression),
							Start, Len, Expression),
						Start, Len, Expression),
					Start, Len, Expression);
			}
			else
				throw new ScriptRuntimeException("Operands not differentiable.", this);
		}

	}
}
