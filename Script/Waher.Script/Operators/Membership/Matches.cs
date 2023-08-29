using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Matches operator
	/// </summary>
	public class Matches : BinaryOperator
    {
		/// <summary>
		/// Matches operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Matches(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Left = this.left.Evaluate(Variables);
            return this.EvaluateMatches(Left, Variables);
        }

        private BooleanValue EvaluateMatches(IElement Left, Variables Variables)
		{
            Dictionary<string, IElement> AlreadyFound = new Dictionary<string, IElement>();

            switch (this.right.PatternMatch(Left, AlreadyFound))
            {
                case PatternMatchResult.Match:
                    foreach (KeyValuePair<string, IElement> P in AlreadyFound)
                        Variables[P.Key] = P.Value;

                    return BooleanValue.True;

                case PatternMatchResult.NoMatch:
                case PatternMatchResult.Unknown:
                default:
                    return BooleanValue.False;
			}
        }

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override async Task<IElement> EvaluateAsync(Variables Variables)
        {
            if (!this.isAsync)
                return this.Evaluate(Variables);
            else
            {
				IElement Left = await this.left.EvaluateAsync(Variables);
				return this.EvaluateMatches(Left, Variables);
			}
        }
    }
}
