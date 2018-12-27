using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Row Vector operator.
	/// </summary>
	public class RowVector : NullCheckBinaryOperator
	{
		/// <summary>
		/// Row Vector operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Y">Y-coordinate operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RowVector(ScriptNode Left, ScriptNode Y, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Left, Y, NullCheck, Start, Length, Expression)
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
			if (this.nullCheck && Left.AssociatedObjectValue is null)
				return Left;

			IElement Right = this.right.Evaluate(Variables);

            return EvaluateIndex(Left, Right, this.nullCheck, this);
        }

		/// <summary>
		/// Evaluates the row index operator.
		/// </summary>
		/// <param name="Matrix">Matrix</param>
		/// <param name="Index">Index</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateIndex(IElement Matrix, IElement Index, bool NullCheck, ScriptNode Node)
        {
			if (Matrix is IMatrix M)
				return EvaluateIndex(M, Index, Node);
            else if (Matrix.IsScalar)
			{
				if (NullCheck && Matrix.AssociatedObjectValue is null)
					return Matrix;

				throw new ScriptRuntimeException("The row index operator operates on matrices.", Node);
			}
			else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E in Matrix.ChildElements)
                    Elements.AddLast(EvaluateIndex(E, Index, NullCheck, Node));

                return Matrix.Encapsulate(Elements, Node);
            }
        }

        /// <summary>
        /// Evaluates the row index operator.
        /// </summary>
        /// <param name="Matrix">Matrix</param>
        /// <param name="Index">Index</param>
        /// <param name="Node">Node performing the operation.</param>
        /// <returns>Result</returns>
        public static IElement EvaluateIndex(IMatrix Matrix, IElement Index, ScriptNode Node)
        {
			if (Index is DoubleNumber RE)
			{
				double d = RE.Value;
                if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
                    throw new ScriptRuntimeException("Row index must be a non-negative integer.", Node);

                return Matrix.GetRow((int)d);
            }

            if (Index.IsScalar)
                throw new ScriptRuntimeException("Row index must be a non-negative integer.", Node);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E in Index.ChildElements)
                    Elements.AddLast(EvaluateIndex(Matrix, E, Node));

                return Index.Encapsulate(Elements, Node);
            }
        }

    }
}
