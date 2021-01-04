using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Integer(x)
    /// </summary>
    public class Integer : FunctionOneScalarVariable
    {
        /// <summary>
        /// Integer(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Integer(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "integer"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "int" }; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(double Argument, Variables Variables)
        {
            return new DoubleNumber(Math.Truncate(Argument));
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(bool Argument, Variables Variables)
        {
            return new DoubleNumber(Argument ? 1 : 0);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
			if (!Expression.TryParse(Argument, out double d))
                throw new ScriptException("Not an integer.");

            return new DoubleNumber(Math.Truncate(d));
		}

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(IElement Argument, Variables Variables)
        {
            return this.EvaluateScalar(Argument.ToString(), Variables);
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        /// <returns>Pattern match result</returns>
        public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            if (!(CheckAgainst is DoubleNumber N))
                return PatternMatchResult.NoMatch;

            double d = N.Value;
            if (Math.Truncate(d) != d)
                return PatternMatchResult.NoMatch;

            return this.Argument.PatternMatch(N, AlreadyFound);
        }
    }
}
