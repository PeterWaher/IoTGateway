using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
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
