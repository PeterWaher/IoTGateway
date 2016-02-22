using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
    /// <summary>
    /// Named function call operator
    /// </summary>
    public class NamedFunctionCall : ScriptNode
    {
        private string functionName;
        private ScriptNode[] arguments;

        /// <summary>
        /// Named function call operator
        /// </summary>
        /// <param name="Function">Function</param>
        /// <param name="Arguments">Arguments</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public NamedFunctionCall(string FunctionName, ScriptNode[] Arguments, int Start, int Length)
            : base(Start, Length)
        {
            this.functionName = FunctionName;
            this.arguments = Arguments;
        }

        /// <summary>
        /// Function name.
        /// </summary>
        public string FunctionName
        {
            get { return this.functionName; }
        }

        /// <summary>
        /// Arguments
        /// </summary>
        public ScriptNode[] Arguments
        {
            get { return this.arguments; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            Variable v;
            string s = this.arguments.Length.ToString();
            LambdaDefinition f;

            if ((!Variables.TryGetVariable(this.functionName + " " + s, out v) &&
               !Variables.TryGetVariable(this.functionName, out v)) ||
               ((f = v.ValueElement as LambdaDefinition) == null))
            {
                throw new ScriptRuntimeException("No function defined with that name and having " + s + " arguments.", this);
            }

            int i, c = this.arguments.Length;
            IElement[] Arguments = new IElement[c];

            for (i = 0; i < c; i++)
                Arguments[i] = this.arguments[i].Evaluate(Variables);

            return f.Evaluate(Arguments, Variables);
        }
    }
}
