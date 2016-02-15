using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Matrix Index operator.
	/// </summary>
	public class MatrixIndex : TernaryOperator
	{
		/// <summary>
		/// Matrix Index operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="X">X-coordinate operand.</param>
		/// <param name="Y">Y-coordinate operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixIndex(ScriptNode Left, ScriptNode X, ScriptNode Y, int Start, int Length)
			: base(Left, X, Y, Start, Length)
		{
		}
	}
}
