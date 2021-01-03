using System;
using Waher.Script.Abstraction.Elements;
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
		/// <param name="Expression">Expression containing script.</param>
		public BinaryDoubleOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
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

			if (L is DoubleNumber DL && R is DoubleNumber DR)
				return this.Evaluate(DL.Value, DR.Value);
			else
				return this.Evaluate(L, R, Variables);
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
			DoubleNumber DL = Left as DoubleNumber;
			DoubleNumber DR = Right as DoubleNumber;

			if (!(DL is null) && !(DR is null))
				return this.Evaluate(DL.Value, DR.Value);
			else
			{
				double l, r;
				PhysicalQuantity Q;

				if (!(DL is null))
					l = DL.Value;
				else
				{
					Q = Left as PhysicalQuantity;
					if (!(Q is null))
						l = Q.Magnitude;
					else
						throw new ScriptRuntimeException("Scalar operands must be double values or physical magnitudes.", this);
				}

				if (!(DR is null))
					r = DR.Value;
				else
				{
					Q = Right as PhysicalQuantity;
					if (!(Q is null))
						r = Q.Magnitude;
					else
						throw new ScriptRuntimeException("Scalar operands must be double values or physical magnitudes.", this);
				}

				return this.Evaluate(l, r);
			}
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
