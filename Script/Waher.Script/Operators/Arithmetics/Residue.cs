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
	/// Residue operator.
	/// </summary>
	public class Residue : BinaryOperator
	{
		/// <summary>
		/// Residue operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Residue(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
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

			return EvaluateResidue(Left, Right, this);
		}

		/// <summary>
		/// Divides the right operand from the left one.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateResidue(IElement Left, IElement Right, ScriptNode Node)
		{
			DoubleNumber DL = Left as DoubleNumber;
			DoubleNumber DR = Right as DoubleNumber;

			if (DL != null && DR != null)
			{
				double dl = DL.Value;
				if (dl < long.MinValue || dl > long.MaxValue || dl != Math.Truncate(dl))
					throw new ScriptRuntimeException("Modulus operator does not work on decimal numbers.", Node);

				double dr = DR.Value;
				if (dr < long.MinValue || dr > long.MaxValue || dr != Math.Truncate(dr))
					throw new ScriptRuntimeException("Modulus operator does not work on decimal numbers.", Node);

				long l = (long)dl;
				long r = (long)dr;

				return new DoubleNumber(l % r);
			}

			if (Left.IsScalar)
			{
				if (Right.IsScalar)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;

					if (!LeftSet.Equals(RightSet))
					{
						if (!Expression.Upgrade(ref Left, ref LeftSet, ref Right, ref RightSet, Node))
							throw new ScriptRuntimeException("Incompatible operands.", Node);

						IEuclidianDomainElement LE = Left as IEuclidianDomainElement;
						IEuclidianDomainElement RE = Right as IEuclidianDomainElement;
						IEuclidianDomainElement Result;

						if (LE != null && RE != null)
						{
							((IEuclidianDomain)LeftSet).Divide(LE, RE, out Result);
							return Result;
						}
					}

					throw new ScriptRuntimeException("Residue could not be computed.", Node);
				}
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluateResidue(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluateResidue(LeftChild, Right, Node));

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
								Elements.AddLast(EvaluateResidue(eLeft.Current, eRight.Current, Node));
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

						foreach (IElement LeftChild in Left.ChildElements)
						{
							LinkedList<IElement> RightResult = new LinkedList<IElement>();

							foreach (IElement RightChild in Right.ChildElements)
								RightResult.AddLast(EvaluateResidue(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

	}
}
