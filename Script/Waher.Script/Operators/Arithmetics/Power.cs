using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Power operator.
	/// </summary>
	public class Power : BinaryOperator
	{
		/// <summary>
		/// Power operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Power(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			return EvaluatePower(Left, Right, this);
		}

		/// <summary>
		/// Calculates Left ^ Right.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluatePower(IElement Left, IElement Right, ScriptNode Node)
		{
			DoubleNumber DL = Left as DoubleNumber;
			DoubleNumber DR = Right as DoubleNumber;

			if (DL != null && DR != null)
				return new DoubleNumber(Math.Pow(DL.Value, DR.Value));

			IRingElement LE = Left as IRingElement;
			if (LE != null && DR != null)
			{
				double d = DR.Value;
				if (d >= long.MinValue && d <= long.MaxValue && Math.Truncate(d) == d)
				{
					long n = (long)d;

					if (n < 0)
					{
						LE = LE.Invert();
						if (LE == null)
							throw new ScriptRuntimeException("Base element not invertible.", Node);

						n = -n;
					}
					else if (n == 0)
					{
						ICommutativeRingWithIdentityElement LE2 = LE as ICommutativeRingWithIdentityElement;
						if (LE2 == null)
							throw new ScriptRuntimeException("Base element ring does not have unity.", Node);

						return LE2.One;
					}

					IRingElement Result = null;

					while (n > 0)
					{
						if ((n & 1) == 1)
						{
							if (Result == null)
								Result = LE;
							else
								Result = (IRingElement)Multiply.EvaluateMultiplication(Result, LE, Node);
						}

						n >>= 1;
						if (n > 0)
							LE = (IRingElement)Multiply.EvaluateMultiplication(LE, LE, Node);
					}

					return Result;
				}
				else
					throw new ScriptRuntimeException("Exponent too large.", Node);
			}

			if (Left.IsScalar)
			{
				if (Right.IsScalar)
					throw new ScriptRuntimeException("Power operation could not be computed.", Node);
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluatePower(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluatePower(LeftChild, Right, Node));

					return Left.Encapsulate(Elements, Node);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						LinkedList<IElement> Elements = new LinkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Elements.AddLast(EvaluatePower(eLeft.Current, eRight.Current, Node));
						}
						finally
						{
							eLeft.Dispose();
							eRight.Dispose();
						}

						return Left.Encapsulate(Elements, Node);
					}
					else
					{
						LinkedList<IElement> LeftResult = new LinkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							LinkedList<IElement> RightResult = new LinkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.AddLast(EvaluatePower(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

	}
}
