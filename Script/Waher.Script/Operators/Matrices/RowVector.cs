using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Row Vector operator.
	/// </summary>
	public class RowVector : BinaryOperator
	{
		/// <summary>
		/// Row Vector operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Y">Y-coordinate operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public RowVector(ScriptNode Left, ScriptNode Y, int Start, int Length)
			: base(Left, Y, Start, Length)
		{
		}
	}
}
