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
	/// Matrix Index operator.
	/// </summary>
	public class MatrixIndex : TernaryOperator
	{
		/// <summary>
		/// Matrix Index operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="X">X-coordinate operand.</param>
		/// <param name="Y">Y-coordinate operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MatrixIndex(ScriptNode Left, ScriptNode X, ScriptNode Y, int Start, int Length, Expression Expression)
			: base(Left, X, Y, Start, Length, Expression)
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
            IElement Middle = this.middle.Evaluate(Variables);
            IElement Right = this.right.Evaluate(Variables);

            return EvaluateIndex(Left, Middle, Right, this);
        }

        /// <summary>
        /// Evaluates the matrix index operator.
        /// </summary>
        /// <param name="Matrix">Matrix</param>
        /// <param name="IndexX">X-Index</param>
        /// <param name="IndexY">Y-Index</param>
        /// <param name="Node">Node performing the operation.</param>
        /// <returns>Result</returns>
        public static IElement EvaluateIndex(IElement Matrix, IElement IndexX, IElement IndexY, ScriptNode Node)
        {
            IMatrix M = Matrix as IMatrix;
            if (M != null)
                return EvaluateIndex(M, IndexX, IndexY, Node);
            else if (Matrix.IsScalar)
                throw new ScriptRuntimeException("The index operator operates on matrices.", Node);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E in Matrix.ChildElements)
                    Elements.AddLast(EvaluateIndex(E, IndexX, IndexY, Node));

                return Matrix.Encapsulate(Elements, Node);
            }
        }

        /// <summary>
        /// Evaluates the vector index operator.
        /// </summary>
        /// <param name="Matrix">Vector</param>
        /// <param name="Index">Index</param>
        /// <param name="Node">Node performing the operation.</param>
        /// <returns>Result</returns>
        public static IElement EvaluateIndex(IMatrix Matrix, IElement IndexX, IElement IndexY, ScriptNode Node)
        {
            DoubleNumber X = IndexX as DoubleNumber;
            DoubleNumber Y = IndexY as DoubleNumber;

            if (X != null && Y != null)
            {
                double x = X.Value;
                double y = Y.Value;

                if (x < 0 || x > int.MaxValue || x != Math.Truncate(x) || y < 0 || y > int.MaxValue || y != Math.Truncate(y))
                    throw new ScriptRuntimeException("Indices must be non-negative integers.", Node);

                return Matrix.GetElement((int)x, (int)y);
            }

            if (IndexX.IsScalar)
            {
                if (IndexY.IsScalar)
                    throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);
                else
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in IndexY.ChildElements)
                        Elements.AddLast(EvaluateIndex(Matrix, IndexX, E, Node));

                    return IndexY.Encapsulate(Elements, Node);
                }
            }
            else
            {
                if (IndexY.IsScalar)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in IndexX.ChildElements)
                        Elements.AddLast(EvaluateIndex(Matrix, E, IndexY, Node));

                    return IndexX.Encapsulate(Elements, Node);
                }
                else
                {
                    ICollection<IElement> IndexXChildren = IndexX.ChildElements;
                    ICollection<IElement> IndexYChildren = IndexY.ChildElements;

                    if (IndexXChildren.Count == IndexYChildren.Count)
                    {
                        LinkedList<IElement> Elements = new LinkedList<IElement>();
                        IEnumerator<IElement> eX = IndexXChildren.GetEnumerator();
                        IEnumerator<IElement> eY = IndexYChildren.GetEnumerator();

                        try
                        {
                            while (eX.MoveNext() && eY.MoveNext())
                                Elements.AddLast(EvaluateIndex(Matrix, eX.Current, eY.Current, Node));
                        }
                        finally
                        {
                            eX.Dispose();
                            eY.Dispose();
                        }

                        return IndexX.Encapsulate(Elements, Node);
                    }
                    else
                    {
                        LinkedList<IElement> XResult = new LinkedList<IElement>();

                        foreach (IElement XChild in IndexXChildren)
                        {
                            LinkedList<IElement> YResult = new LinkedList<IElement>();

                            foreach (IElement YChild in IndexYChildren)
                                YResult.AddLast(EvaluateIndex(Matrix, XChild, YChild, Node));

                            XResult.AddLast(IndexY.Encapsulate(YResult, Node));
                        }

                        return IndexX.Encapsulate(XResult, Node);
                    }
                }
            }
        }

    }
}
