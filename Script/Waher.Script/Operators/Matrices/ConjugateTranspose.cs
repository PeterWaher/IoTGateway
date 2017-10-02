using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Conjugate Transpose operator.
	/// </summary>
	public class ConjugateTranspose : UnaryOperator 
	{
		/// <summary>
		/// Conjugate Transpose operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ConjugateTranspose(ScriptNode Operand, int Start, int Length, Expression Expression)
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
                return Matrix.ConjugateTranspose();

            if (Operand is IVector Vector)
                return MatrixDefinition.Encapsulate(Vector.VectorElements, 1, Vector.Dimension, this).ConjugateTranspose();

            return Operand;
        }
    }
}
