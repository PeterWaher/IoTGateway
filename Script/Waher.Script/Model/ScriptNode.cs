using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for all nodes in a parsed script tree.
    /// </summary>
    public abstract class ScriptNode
    {
		private Expression expression;
        private int start;
        private int length;

        /// <summary>
        /// Base class for all nodes in a parsed script tree.
        /// </summary>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public ScriptNode(int Start, int Length, Expression Expression)
        {
            this.start = Start;
            this.length = Length;
			this.expression = Expression;
        }

        /// <summary>
        /// Start position in script expression.
        /// </summary>
        public int Start
        {
            get { return this.start; }
            internal set { this.start = value; }
        }

        /// <summary>
        /// Length of expression covered by node.
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public abstract IElement Evaluate(Variables Variables);

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        public virtual void PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            throw new ScriptRuntimeException("Pattern mismatch.", this);
        }

		/// <summary>
		/// Expression of which the node is a part.
		/// </summary>
		public Expression Expression
		{
			get { return this.expression; }
		}

		/// <summary>
		/// Sub-expression defining the node.
		/// </summary>
		public string SubExpression
		{
			get { return this.expression.Script.Substring(this.start, this.length); }
		}
    }
}
