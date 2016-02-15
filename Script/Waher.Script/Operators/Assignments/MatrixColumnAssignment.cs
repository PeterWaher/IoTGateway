using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Matrix Column Assignment operator.
	/// </summary>
	public class MatrixColumnAssignment : UnaryOperator 
	{
		ColumnVector matrixColumn;

		/// <summary>
		/// Matrix Column Assignment operator.
		/// </summary>
		/// <param name="MatrixColumn">Matrix Column</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixColumnAssignment(ColumnVector MatrixColumn, ScriptNode Operand, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.matrixColumn = MatrixColumn;
		}

		/// <summary>
		/// Matrix Column.
		/// </summary>
		public ColumnVector MatrixColumn
		{
			get { return this.matrixColumn; }
		}

	}
}
