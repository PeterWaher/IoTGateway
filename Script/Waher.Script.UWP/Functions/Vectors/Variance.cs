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
    /// Variance(v), Var(v)
    /// </summary>
    public class Variance : FunctionOneVectorVariable
    {
        /// <summary>
        /// Variance(v), Var(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Variance(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "variance"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "var" }; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            return new DoubleNumber(CalcVariance(Argument.Values, this));
        }

        /// <summary>
        /// Calculates the variance of a set of double values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Variance.</returns>
        public static double CalcVariance(double[] Values, ScriptNode Node)
        {
            double Avg = Average.CalcAverage(Values, Node);
            double Result = 0;
            double d;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
            {
                d = (Values[i] - Avg);
                Result += d * d;
            }

            Result /= c;

            return Result;
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
            return new ComplexNumber(CalcVariance(Argument.Values, this));
        }

        /// <summary>
        /// Calculates the variance of a set of complex values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Variance.</returns>
        public static Complex CalcVariance(Complex[] Values, ScriptNode Node)
        {
            Complex Avg = Average.CalcAverage(Values, Node);
            Complex Result = 0;
            Complex d;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
            {
                d = (Values[i] - Avg);
                Result += d * d;
            }

            Result /= c;

            return Result;
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
