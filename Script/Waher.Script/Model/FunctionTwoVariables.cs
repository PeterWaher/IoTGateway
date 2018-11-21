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
		/// <param name="Expression">Expression containing script.</param>
        public FunctionTwoVariables(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
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

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!this.argument1.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.argument2.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			if (!Callback(ref this.argument1, State))
				return false;

			if (!Callback(ref this.argument2, State))
				return false;

			if (!DepthFirst)
			{
				if (!this.argument1.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.argument2.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			return true;
		}

	}
}
