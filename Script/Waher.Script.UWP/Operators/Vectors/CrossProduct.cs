using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Cross-product operator.
	/// </summary>
	public class CrossProduct : BinaryVectorOperator
	{
		/// <summary>
		/// Cross-product operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public CrossProduct(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the operator on vector operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateVector(IVector Left, IVector Right)
		{
			if (Left.Dimension != 3 || Right.Dimension != 3)
				throw new ScriptRuntimeException("Cross product works on vectors of dimesion 3.", this);

			DoubleVector dv1 = Left as DoubleVector;
			DoubleVector dv2 = Right as DoubleVector;

			if (dv1 != null && dv2 != null)
			{
				double[] d1 = dv1.Values;
				double[] d2 = dv2.Values;

				return new DoubleVector(new double[] { d1[1] * d2[2] - d1[2] * d2[1], d1[2] * d2[0] - d1[0] * d2[2], d1[0] * d2[1] - d1[1] * d2[0] });
			}

			IElement[] v1 = new IElement[3];
			Left.VectorElements.CopyTo(v1, 0);

			IElement[] v2 = new IElement[3];
			Right.VectorElements.CopyTo(v2, 0);

			return VectorDefinition.Encapsulate(new IElement[]
			{
				Operators.Arithmetics.Subtract.EvaluateSubtraction(
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[1], v2[2], this),
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[2], v2[1], this), this),
				Operators.Arithmetics.Subtract.EvaluateSubtraction(
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[2], v2[0], this), 
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[0], v2[2], this), this),
				Operators.Arithmetics.Subtract.EvaluateSubtraction(
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[0], v2[1], this), 
					Operators.Arithmetics.Multiply.EvaluateMultiplication(v1[1], v2[0], this), this)
			}, false, this);
		}

	}
}
