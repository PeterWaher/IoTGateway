using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Operators.Assignments
{
	/// <summary>
	/// Matrix Column Assignment operator.
	/// </summary>
	public class MatrixColumnAssignment : TernaryOperator
    {
		/// <summary>
		/// Matrix Column Assignment operator.
		/// </summary>
		/// <param name="MatrixColumn">Matrix Column</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixColumnAssignment(ColumnVector MatrixColumn, ScriptNode Operand, int Start, int Length, Expression Expression)
            : base(MatrixColumn.LeftOperand, MatrixColumn.RightOperand, Operand, Start, Length, Expression)
        {
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Left = this.left.Evaluate(Variables);
            IMatrix M = Left as IMatrix;
            if (M == null)
                throw new ScriptRuntimeException("Matrix column vector assignment can only be performed on matrices.", this);

            IElement Index = this.middle.Evaluate(Variables);
            DoubleNumber IE = Index as DoubleNumber;
            double d;

            if (IE == null || (d = IE.Value) < 0 || d > int.MaxValue || d != Math.Truncate(d))
                throw new ScriptRuntimeException("Index must be a non-negative integer.", this);

            IElement Value = this.right.Evaluate(Variables);
            IVector V = Value as IVector;
            if (V == null)
                throw new ScriptRuntimeException("Matrix columns must be vectors.", this);

            if (M.Rows != V.Dimension)
                throw new ScriptRuntimeException("Vector dimension does not match number of rows in matrix.", this);

            M.SetColumn((int)d, V);

            return Value;
        }

    }
}
