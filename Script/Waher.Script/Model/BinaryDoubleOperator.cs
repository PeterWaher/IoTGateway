using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for binary double operators.
	/// </summary>
	public abstract class BinaryDoubleOperator : BinaryScalarOperator
	{
		/// <summary>
		/// Base class for binary double operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public BinaryDoubleOperator(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement L = this.left.Evaluate(Variables);
			IElement R = this.right.Evaluate(Variables);
			DoubleNumber DL = L as DoubleNumber;
			DoubleNumber DR = R as DoubleNumber;

			if (DL != null && DR != null)
				return this.Evaluate(DL.Value, DR.Value);
			else
				return this.Evaluate(L, R);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Left, IElement Right)
		{
			DoubleNumber DL = Left as DoubleNumber;
			DoubleNumber DR = Right as DoubleNumber;

			if (DL != null && DR != null)
				return this.Evaluate(DL.Value, DR.Value);
			else
				throw new ScriptRuntimeException("Scalar operands must be double values.", this);
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(double Left, double Right);

	}
}
