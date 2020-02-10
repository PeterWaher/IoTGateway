using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Operators.Assignments
{
    /// <summary>
    /// Matrix Index Assignment operator.
    /// </summary>
    public class MatrixIndexAssignment : QuaternaryOperator
    {
        /// <summary>
        /// Matrix Index Assignment operator.
        /// </summary>
        /// <param name="MatrixIndex">Matrix Index</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public MatrixIndexAssignment(MatrixIndex MatrixIndex, ScriptNode Operand, int Start, int Length, Expression Expression)
            : base(MatrixIndex.LeftOperand, MatrixIndex.MiddleOperand, MatrixIndex.RightOperand, Operand, Start, Length, Expression)
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
            if (!(Left is IMatrix M))
                throw new ScriptRuntimeException("Matrix element assignment can only be performed on matrices.", this);

            IElement ColIndex = this.middle.Evaluate(Variables);
            IElement RowIndex = this.middle2.Evaluate(Variables);
            double x, y;

            if (!(ColIndex is DoubleNumber X) || (x = X.Value) < 0 || x > int.MaxValue || x != Math.Truncate(x) ||
                !(RowIndex is DoubleNumber Y) || (y = Y.Value) < 0 || y > int.MaxValue || y != Math.Truncate(y))
            {
                throw new ScriptRuntimeException("Indices must be non-negative integers.", this);
            }

            IElement Value = this.right.Evaluate(Variables);

            M.SetElement((int)x, (int)y, Value);

            return Value;
        }

    }
}
