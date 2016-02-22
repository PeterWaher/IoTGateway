using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Model
{
    /// <summary>
    /// Represents a constant element value.
    /// </summary>
    public class ConstantElement : ScriptNode
    {
        private IElement constant;

        /// <summary>
        /// Represents a constant element value.
        /// </summary>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public ConstantElement(IElement Constant, int Start, int Length)
            : base(Start, Length)
        {
            this.constant = Constant;
        }

        /// <summary>
        /// Constant value.
        /// </summary>
        public IElement Constant
        {
            get { return this.constant; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            return this.constant;
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        public override void PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            if (!this.constant.Equals(CheckAgainst))
                throw new ScriptRuntimeException("Pattern mismatch.", this);
        }
    }
}
