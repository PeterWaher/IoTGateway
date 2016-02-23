using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one variable.
    /// </summary>
    public abstract class FunctionTwoVariables : Function
	{
        private ScriptNode argument1;
        private ScriptNode argument2;

        /// <summary>
        /// Base class for funcions of one variable.
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionTwoVariables(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length)
			: base(Start, Length)
		{
            this.argument1 = Argument1;
            this.argument2 = Argument2;
        }

        /// <summary>
        /// Function argument 1.
        /// </summary>
        public ScriptNode Argument1
        {
            get { return this.argument1; }
        }

        /// <summary>
        /// Function argument 2.
        /// </summary>
        public ScriptNode Argument2
        {
            get { return this.argument2; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
		{
            get { return new string[] { "x", "y" }; }
		}

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Arg1 = this.argument1.Evaluate(Variables);
            IElement Arg2 = this.argument2.Evaluate(Variables);

            return this.Evaluate(Arg1, Arg2, Variables);
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables);

    }
}
