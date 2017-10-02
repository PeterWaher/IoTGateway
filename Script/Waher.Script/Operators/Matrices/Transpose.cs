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
		/// <param name="Expression">Expression containing script.</param>
        public Transpose(ScriptNode Operand, int Start, int Length, Expression Expression)
            : base(Operand, Start, Length, Expression)
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
            if (Operand is IMatrix Matrix)
                return Matrix.Transpose();

			if (Operand is IVector Vector)
				return MatrixDefinition.Encapsulate(Vector.VectorElements, Vector.Dimension, 1, this);

            return Operand;
        }
    }
}
