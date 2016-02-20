using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Elements.Interfaces;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
    /// <summary>
    /// To-Matrix operator.
    /// </summary>
    public class ToMatrix : UnaryOperator
    {
        /// <summary>
        /// To-Matrix operator.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public ToMatrix(ScriptNode Operand, int Start, int Length)
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
            IElement E = this.op.Evaluate(Variables);

            if (E is IMatrix)
                return E;

            IVector V = E as IVector;
            if (V != null)
                return MatrixDefinition.Encapsulate(V.VectorElements, 1, V.Dimension, this);

            ISet S = E as ISet;
            if (S != null)
            {
                ICollection<IElement> Elements = S.ChildElements;
                return MatrixDefinition.Encapsulate(Elements, 1, Elements.Count, this);
            }

            return MatrixDefinition.Encapsulate(new IElement[] { E }, 1, 1, this);
        }
    }
}
