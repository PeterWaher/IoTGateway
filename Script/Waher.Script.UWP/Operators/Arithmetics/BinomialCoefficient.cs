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
    /// Binomial coefficient.
    /// </summary>
    public class BinomialCoefficient : BinaryDoubleOperator
    {
        /// <summary>
        /// Binomial coefficient.
        /// </summary>
        /// <param name="Left">Left operand.</param>
        /// <param name="Right">Right operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public BinomialCoefficient(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
		{
        }

        /// <summary>
        /// Evaluates the double operator.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(double Left, double Right)
        {
            if (Left < 0 || Left > int.MaxValue || Right < 0 || Right > int.MaxValue || Left != Math.Truncate(Left) || Right != Math.Truncate(Right))
                throw new ScriptRuntimeException("Expected non-negative integers.", this);

            int n = (int)Left;
            int k = (int)Right;
            int n_k = n - k;
            if (n_k < 0)
                throw new ScriptRuntimeException("The Denominator in the OVER operator, cannot be larger than the numerator", this);

            double Result = 1;

            int Max = Math.Max(k, n_k);
            int Min = Math.Min(k, n_k);

            while (n > Max)
            {
                Result *= n;
                n--;

                while (Result > 1 && Min > 1)
                {
                    Result /= Min;
                    Min--;
                }
            }

            while (Min > 1)
            {
                Result /= Min;
                Min--;
            }

            return new DoubleNumber(Math.Round(Result));    // Any decimals are a result of rounding errors induced by the algorithm.
        }

    }
}
