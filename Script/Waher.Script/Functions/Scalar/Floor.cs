using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Floor(x)
    /// </summary>
    public class Floor : FunctionOneScalarVariable
    {
        /// <summary>
        /// Floor(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Floor(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Floor);

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(double Argument, Variables Variables)
        {
            return new DoubleNumber(Math.Floor(Argument));
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(Complex Argument, Variables Variables)
        {
            return new ComplexNumber(new Complex(Math.Floor(Argument.Real), Math.Floor(Argument.Imaginary)));
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        /// <returns>Pattern match result</returns>
        public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            if (CheckAgainst is DoubleNumber N)
            {
                double d = N.Value;
                if (Math.Truncate(d) != d)
                    return PatternMatchResult.NoMatch;
            }
            else if (CheckAgainst is ComplexNumber Z)
            {
                Complex z = Z.Value;
                double d = z.Real;
                if (Math.Truncate(d) != d)
                    return PatternMatchResult.NoMatch;

                d = z.Imaginary;
                if (Math.Truncate(d) != d)
                    return PatternMatchResult.NoMatch;
            }
            else
                return PatternMatchResult.NoMatch;

            return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
        }
    }
}
