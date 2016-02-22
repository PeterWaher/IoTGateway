using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
    /// <summary>
    /// Not-In operator
    /// </summary>
    public class NotIn : In
    {
        /// <summary>
        /// Not-In operator
        /// </summary>
        /// <param name="Left">Left operand.</param>
        /// <param name="Right">Right operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public NotIn(ScriptNode Left, ScriptNode Right, int Start, int Length)
            : base(Left, Right, Start, Length)
        {
        }

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <returns>Result</returns>
        public override IElement Evaluate(IElement Left, ISet Right)
        {
            if (Right.Contains(Left))
                return BooleanValue.False;
            else
                return BooleanValue.True;
        }
    }
}
