using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Dynamic function call operator
	/// </summary>
	public class DynamicFunctionCall : UnaryOperator 
	{
		private ScriptNode[] arguments;

		/// <summary>
		/// Dynamic function call operator
		/// </summary>
		/// <param name="Function">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DynamicFunctionCall(ScriptNode Function, ScriptNode[] Arguments, int Start, int Length)
			: base(Function, Start, Length)
		{
			this.arguments = Arguments;
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
		public override Element Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}
	}
}
