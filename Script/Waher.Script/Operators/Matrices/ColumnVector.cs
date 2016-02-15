using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Column Vector operator.
	/// </summary>
	public class ColumnVector : BinaryOperator
	{
		/// <summary>
		/// Column Vector operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="X">X-coordinate operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ColumnVector(ScriptNode Left, ScriptNode X, int Start, int Length)
			: base(Left, X, Start, Length)
		{
		}
	}
}
