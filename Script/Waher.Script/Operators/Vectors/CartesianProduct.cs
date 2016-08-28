using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Cartesian-product operator.
	/// </summary>
	public class CartesianProduct : BinaryVectorOperator
	{
		/// <summary>
		/// Cartesian-product operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public CartesianProduct(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the operator on vector operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateVector(IVector Left, IVector Right)
		{
			if (Left.Dimension != Right.Dimension)
				throw new ScriptRuntimeException("Vectors of different dimensions.", this);

			LinkedList<IElement> Elements = new LinkedList<IElement>();
			IEnumerator<IElement> e1 = Left.VectorElements.GetEnumerator();
			IEnumerator<IElement> e2 = Right.VectorElements.GetEnumerator();
			IElement v1 = null;

			while (e1.MoveNext())
			{
				v1 = e1.Current;
				while (e2.MoveNext())
					Elements.AddLast(VectorDefinition.Encapsulate(new IElement[] { v1, e2.Current }, false, this));

				e2.Reset();
			}

			return new ObjectVector(Elements);
		}
	}
}
