using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Comparisons
{
    /// <summary>
    /// Not Like
    /// </summary>
    public class NotLike : Like
    {
        /// <summary>
        /// Not Like
        /// </summary>
        /// <param name="Left">Left operand.</param>
        /// <param name="Right">Right operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public NotLike(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
            IElement Result = base.EvaluateScalar(Left, Right, Variables);
            BooleanValue B = (BooleanValue)Result;
            if (B.Value)
                return BooleanValue.False;
            else
                return BooleanValue.True;
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        /// <returns>Pattern match result</returns>
        public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            switch (base.PatternMatch(CheckAgainst, AlreadyFound))
            {
                case PatternMatchResult.Match:
                    return PatternMatchResult.NoMatch;

                case PatternMatchResult.NoMatch: 
                    return PatternMatchResult.Match;

                case PatternMatchResult.Unknown: 
                default:
                    return PatternMatchResult.Unknown;
            }
        }
    }
}
