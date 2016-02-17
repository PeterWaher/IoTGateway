using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// Try-Catch operator.
	/// </summary>
	public class TryCatch : BinaryOperator
	{
		/// <summary>
		/// Try-Catch operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="CatchStatement">Catch statement.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public TryCatch(ScriptNode Statement, ScriptNode CatchStatement, int Start, int Length)
			: base(Statement, CatchStatement, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}
	}
}
