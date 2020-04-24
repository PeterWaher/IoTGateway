using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Operators;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of zero variables.
    /// </summary>
    public abstract class FunctionZeroVariables : Function
	{
        /// <summary>
        /// Base class for funcions of one variable.
        /// </summary>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public FunctionZeroVariables(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
            get { return new string[0]; }
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			return true;
		}

	}
}
