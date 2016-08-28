using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
    /// <summary>
    /// Assignment operator.
    /// </summary>
    public class PatternMatch : BinaryOperator
    {
        /// <summary>
        /// Assignment operator.
        /// </summary>
        /// <param name="Left">Left Operand.</param>
        /// <param name="Right">Right Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public PatternMatch(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
            : base(Left, Right, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Result = this.right.Evaluate(Variables);
            Dictionary<string, IElement> AlreadyFound = new Dictionary<string, IElement>();

            this.left.PatternMatch(Result, AlreadyFound);

            foreach (KeyValuePair<string, IElement> P in AlreadyFound)
                Variables[P.Key] = P.Value;

            return Result;
        }
    }
}
