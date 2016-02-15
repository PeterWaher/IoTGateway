using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// DO-WHILE operator.
	/// </summary>
	public class DoWhile : BinaryOperator
	{
		/// <summary>
		/// DO-WHILE operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="Condition">Condition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DoWhile(ScriptNode Statement, ScriptNode Condition, int Start, int Length)
			: base(Statement, Condition, Start, Length)
		{
		}
	}
}
