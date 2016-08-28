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
    /// Destroys a value. If the function references a variable, the variable is also removed.
    /// </summary>
    public class Destroy : FunctionOneVariable
    {
        private string variableName = string.Empty;

        /// <summary>
        /// Destroys a value. If the function references a variable, the variable is also removed.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Destroy(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
            VariableReference Ref = Argument as VariableReference;
            if (Ref != null)
                this.variableName = Ref.VariableName;
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
            get { return "destroy"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "delete" }; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Element;

            if (!string.IsNullOrEmpty(this.variableName))
            {
                Variable v;

                if (Variables.TryGetVariable(this.variableName, out v))
                {
                    Variables.Remove(this.variableName);
                    Element = v.ValueElement;
                }
                else
                    Element = null;
            }
            else
                Element = this.Argument.Evaluate(Variables);

            if (Element != null)
            {
                IDisposable D = Element.AssociatedObjectValue as IDisposable;
                if (D != null)
                    D.Dispose();
            }

            return ObjectValue.Null;
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return ObjectValue.Null;
        }
    }
}
