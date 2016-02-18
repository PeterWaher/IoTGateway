using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Dot-product operator.
	/// </summary>
	public class DotProduct : BinaryVectorOperator
	{
		/// <summary>
		/// Dot-product operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DotProduct(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
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
			if (Left.Dimension != Right.Dimension)
				throw new ScriptRuntimeException("Vectors of different dimensions.", this);

			IEnumerator<IElement> e1 = Left.VectorElements.GetEnumerator();
			IEnumerator<IElement> e2 = Right.VectorElements.GetEnumerator();
			IElement Result = null;

			while (e1.MoveNext() && e2.MoveNext())
			{
				if (Result == null)
					Result = Operators.Arithmetics.Multiply.EvaluateMultiplication(e1.Current, e2.Current, this);
				else
				{
					Result = Operators.Arithmetics.Add.EvaluateAddition(Result,
						Operators.Arithmetics.Multiply.EvaluateMultiplication(e1.Current, e2.Current, this), this);
				}
			}

			if (Result == null)
				throw new ScriptRuntimeException("Cannot operate on zero-dimension vectors.", this);

			return Result;
		}

	}
}
