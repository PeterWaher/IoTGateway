using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
    /// <summary>
    /// Transpose operator.
    /// </summary>
    public class Transpose : UnaryOperator
    {
        /// <summary>
        /// Transpose operator.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Transpose(ScriptNode Operand, int Start, int Length)
            : base(Operand, Start, Length)
        {
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Operand = this.op.Evaluate(Variables);
            IMatrix Matrix = Operand as IMatrix;
            if (Matrix != null)
                return Matrix.Transpose();

            IVector Vector = Operand as IVector;
            if (Vector != null)
                return MatrixDefinition.Encapsulate(Vector.VectorElements, Vector.Dimension, 1, this);

            return Operand;
        }
    }
}
