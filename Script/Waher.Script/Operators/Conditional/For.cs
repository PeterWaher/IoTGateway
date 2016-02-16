using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// FOR operator.
	/// </summary>
	public class For : QuaternaryOperator
	{
		private string variableName;

		/// <summary>
		/// FOR operator.
		/// </summary>
		/// <param name="From">Required From statement.</param>
		/// <param name="To">Required To statement.</param>
		/// <param name="Step">Optional Step statement.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public For(string VariableName, ScriptNode From, ScriptNode To, ScriptNode Step, ScriptNode Statement, int Start, int Length)
			: base(From, To, Step, Statement, Start, Length)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Variable Name.
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Element Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}
	}
}
