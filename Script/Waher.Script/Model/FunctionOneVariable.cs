using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one variable.
    /// </summary>
    public abstract class FunctionOneVariable : Function
	{
        private ScriptNode argument;

        /// <summary>
        /// Base class for funcions of one variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionOneVariable(ScriptNode Argument, int Start, int Length)
			: base(Start, Length)
		{
            this.argument = Argument;
		}

        /// <summary>
        /// Function argument.
        /// </summary>
        public ScriptNode Argument
        {
            get { return this.argument; }
        }

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
            get { return new string[] { "x" }; }
		}

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Arg = this.argument.Evaluate(Variables);
            return this.Evaluate(Arg, Variables);
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement Evaluate(IElement Argument, Variables Variables);

    }
}
