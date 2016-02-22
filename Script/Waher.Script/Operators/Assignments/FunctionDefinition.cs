using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Assignments
{
    /// <summary>
    /// Function definition operator.
    /// </summary>
    public class FunctionDefinition : LambdaDefinition
    {
        private string functionName;

        /// <summary>
        /// Function definition operator.
        /// </summary>
        /// <param name="FunctionName">Function name..</param>
        /// <param name="ArgumentNames">Argument names.</param>
        /// <param name="ArgumentTypes">Argument types.</param>
        /// <param name="Body">Function body.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionDefinition(string FunctionName, string[] ArgumentNames, ArgumentType[] ArgumentTypes, ScriptNode Body, int Start, int Length)
            : base(ArgumentNames, ArgumentTypes, Body, Start, Length)
        {
            this.functionName = FunctionName;
        }

        /// <summary>
        /// Name of function
        /// </summary>
        public string FunctionName
        {
            get { return this.functionName; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            Variables[this.functionName + " " + this.ArgumentNames.Length.ToString()] = this;
            return this;
        }

    }
}
