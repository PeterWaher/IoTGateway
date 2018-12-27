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
        private readonly string variableName;

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
            if (Argument is null)
                this.variableName = string.Empty;
            else
            {
				if (!(Argument is VariableReference Ref))
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
			if (Variables.TryGetVariable(this.variableName, out Variable v))
			{
				Variables.Remove(this.variableName);
				return v.ValueElement;
			}
			else
				return ObjectValue.Null;
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

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Remove O &&
				this.variableName.Equals(O.variableName) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.variableName.GetHashCode();
			return Result;
		}
	}
}
