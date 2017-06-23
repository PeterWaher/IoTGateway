using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Removes a variable from the variables collection, without destroying its value.
    /// </summary>
    public class Remove : Function
    {
        private string variableName;

        /// <summary>
        /// Removes a variable from the variables collection, without destroying its value.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Remove(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
            if (Argument == null)
                this.variableName = string.Empty;
            else
            {
                VariableReference Ref = Argument as VariableReference;
                if (Ref == null)
                    throw new SyntaxException("Variable reference expected.", Argument.Start, string.Empty);

                this.variableName = Ref.VariableName;
            }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "var" }; }
        }

        /// <summary>
        /// Name of variable.
        /// </summary>
        public string VariableName
        {
            get { return this.variableName; }
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "remove"; }
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
            {
                Variables.Remove(this.variableName);
                return v.ValueElement;
            }
            else
                return ObjectValue.Null;
        }
    }
}
