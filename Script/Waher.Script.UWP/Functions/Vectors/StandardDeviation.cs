using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// StandardDeviation(v), StdDev(v)
    /// </summary>
    public class StandardDeviation : FunctionOneVectorVariable
    {
        /// <summary>
        /// StandardDeviation(v), StdDev(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public StandardDeviation(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "standarddeviation"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "stddev" }; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            return new DoubleNumber(CalcStandardDeviation(Argument.Values, this));
        }

        /// <summary>
        /// Calculates the standard deviation of a set of double values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Standard deviation.</returns>
        public static double CalcStandardDeviation(double[] Values, ScriptNode Node)
        {
            double Avg = Average.CalcAverage(Values, Node);
            double Result = 0;
            double d;
            int i, c = Values.Length;

            if (c == 1)
                return 0;

            for (i = 0; i < c; i++)
            {
                d = (Values[i] - Avg);
                Result += d * d;
            }

            Result /= (c - 1);

            return Math.Sqrt(Result);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
            return new ComplexNumber(CalcStandardDeviation(Argument.Values, this));
        }

        /// <summary>
        /// Calculates the standard deviation of a set of complex values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Standard deviation.</returns>
        public static Complex CalcStandardDeviation(Complex[] Values, ScriptNode Node)
        {
            Complex Avg = Average.CalcAverage(Values, Node);
            Complex Result = 0;
            Complex d;
            int i, c = Values.Length;

            if (c == 1)
                return Complex.Zero;

            for (i = 0; i < c; i++)
            {
                d = (Values[i] - Avg);
                Result += d * d;
            }

            Result /= (c - 1);

            return Complex.Sqrt(Result);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("Expected a numeric vector.", this);
        }

    }
}
