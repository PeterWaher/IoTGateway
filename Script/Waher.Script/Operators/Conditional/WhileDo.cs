using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// WHILE-DO operator.
	/// </summary>
	public class WhileDo : BinaryOperator
	{
		/// <summary>
		/// WHILE-DO operator.
		/// </summary>
		/// <param name="Condition">Condition.</param>
		/// <param name="Statement">Statement.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public WhileDo(ScriptNode Condition, ScriptNode Statement, int Start, int Length)
			: base(Condition, Statement, Start, Length)
		{
		}
	}
}
