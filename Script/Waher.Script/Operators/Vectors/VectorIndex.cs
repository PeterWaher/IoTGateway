using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Matrices;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Vector Index operator.
	/// </summary>
	public class VectorIndex : BinaryOperator 
	{
		/// <summary>
		/// Vector Index operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorIndex(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
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
            IElement Right = this.right.Evaluate(Variables);

            return EvaluateIndex(Left, Right, this);
        }

        /// <summary>
        /// Evaluates the vector index operator.
        /// </summary>
        /// <param name="Vector">Vector</param>
        /// <param name="Index">Index</param>
        /// <param name="Node">Node performing the operation.</param>
        /// <returns>Result</returns>
        public static IElement EvaluateIndex(IElement Vector, IElement Index, ScriptNode Node)
        {
            IVector V = Vector as IVector;
            if (V != null)
                return EvaluateIndex(V, Index, Node);
            else if (Vector.IsScalar)
                throw new ScriptRuntimeException("The index operator operates on vectors.", Node);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E in Vector.ChildElements)
                    Elements.AddLast(EvaluateIndex(E, Index, Node));

                return Vector.Encapsulate(Elements, Node);
            }
        }

        /// <summary>
        /// Evaluates the vector index operator.
        /// </summary>
        /// <param name="Vector">Vector</param>
        /// <param name="Index">Index</param>
        /// <param name="Node">Node performing the operation.</param>
        /// <returns>Result</returns>
        public static IElement EvaluateIndex(IVector Vector, IElement Index, ScriptNode Node)
        {
            DoubleNumber RE = Index as DoubleNumber;

            if (RE != null)
            {
                double d = RE.Value;
                if (d < 0 || d > int.MaxValue || d != Math.Truncate(d))
                    throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);

                return Vector.GetElement((int)d);
            }

            if (Index.IsScalar)
                throw new ScriptRuntimeException("Index must be a non-negative integer.", Node);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();
                
                foreach (IElement E in Index.ChildElements)
                    Elements.AddLast(EvaluateIndex(Vector, E, Node));

                return Index.Encapsulate(Elements, Node);
            }
        }

    }
}
