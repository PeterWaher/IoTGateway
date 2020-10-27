using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Makes sure an expression is defined. Otherwise, an exception is thrown.
    /// </summary>
    public class Optional : FunctionOneVariable
    {
        /// <summary>
        /// Makes sure an expression is defined. Otherwise, an exception is thrown.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public Optional(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "optional"; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            try
            {
                return this.Argument.Evaluate(Variables);
            }
            catch (Exception)
			{
                return ObjectValue.Null;
			}
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
            return Argument;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
            if (CheckAgainst is ObjectValue V && V.AssociatedObjectValue is null)
            {
                this.Argument.ForAllChildNodes((ref ScriptNode Node, object State) =>
                {
                    if (Node is VariableReference Ref && !AlreadyFound.ContainsKey(Ref.VariableName))
                        AlreadyFound[Ref.VariableName] = ObjectValue.Null;

                    return true;
                }, null, true);

                return PatternMatchResult.Match;
            }
            else
                return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
		}
	}
}
