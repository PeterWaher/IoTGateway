using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// Try-Finally operator.
	/// </summary>
	public class TryFinally : BinaryOperator
	{
		/// <summary>
		/// Try-Finally operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="FinallyStatement">Finally statement.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public TryFinally(ScriptNode Statement, ScriptNode FinallyStatement, int Start, int Length)
			: base(Statement, FinallyStatement, Start, Length)
		{
		}
	}
}
