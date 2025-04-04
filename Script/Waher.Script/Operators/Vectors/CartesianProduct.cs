﻿using System.Collections.Generic;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

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
		/// <param name="Expression">Expression containing script.</param>
		public CartesianProduct(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the operator on vector operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateVector(IVector Left, IVector Right, Variables Variables)
		{
			if (Left.Dimension != Right.Dimension)
				throw new ScriptRuntimeException("Vectors of different dimensions.", this);

			ChunkedList<IElement> Elements = new ChunkedList<IElement>();
			IEnumerator<IElement> e1 = Left.VectorElements.GetEnumerator();
			IEnumerator<IElement> e2 = Right.VectorElements.GetEnumerator();
			IElement v1;

			while (e1.MoveNext())
			{
				v1 = e1.Current;
				while (e2.MoveNext())
					Elements.Add(VectorDefinition.Encapsulate(new IElement[] { v1, e2.Current }, false, this));

				e2.Reset();
			}

			return VectorDefinition.Encapsulate(Elements, false, this);
		}
	}
}
