using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Creates a matrix.
	/// </summary>
	public class MatrixDefinition : ElementList
	{
		/// <summary>
		/// Creates a matrix.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixDefinition(ScriptNode[] Rows, int Start, int Length)
			: base(Rows, Start, Length)
		{
		}
	}
}
