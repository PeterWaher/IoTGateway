using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
    /// <summary>
    /// Represents a variable reference.
    /// </summary>
    public sealed class VariableReference : ScriptNode
    {
        private string variableName;
        private bool onlyVariables;

        /// <summary>
        /// Represents a variable reference.
        /// </summary>
        /// <param name="VariableName">Variable name.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public VariableReference(string VariableName, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
            this.variableName = VariableName;
            this.onlyVariables = false;
        }

        /// <summary>
        /// Represents a variable reference.
        /// </summary>
        /// <param name="VariableName">Variable name.</param>
        /// <param name="OnlyVariables">If only values of variables should be returned (true), or if constants and namespaces should
        /// also be included in the scope of the reference (false).</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public VariableReference(string VariableName, bool OnlyVariables, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
            this.variableName = VariableName;
            this.onlyVariables = OnlyVariables;
        }

        /// <summary>
        /// Variable Name.
        /// </summary>
        public string VariableName
        {
            get { return this.variableName; }
        }

        /// <summary>
        /// If only values of variables should be returned (true), or if constants and namespaces should
        /// also be included in the scope of the reference (false).
        /// </summary>
        public bool OnlyVariables
        {
            get { return this.onlyVariables; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            Variable v;

            if (Variables.TryGetVariable(this.variableName, out v))
                return v.ValueElement;

            if (!this.onlyVariables)
            {
                IElement ValueElement;

                if (Expression.TryGetConstant(this.variableName, out ValueElement))
                    return ValueElement;

                if (Types.IsRootNamespace(this.variableName))
                    return new Namespace(this.variableName);

				ValueElement = Expression.GetFunctionLambdaDefinition(this.variableName, this.Start, this.Length, this.Expression);
				if (ValueElement != null)
					return ValueElement;
            }

            throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        public override void PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            IElement E;

            if (AlreadyFound.TryGetValue(this.variableName,out E) && !E.Equals(CheckAgainst))
                throw new ScriptRuntimeException("Pattern mismatch.", this);

            AlreadyFound[this.variableName] = CheckAgainst;
        }

    }
}
