using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Left-Division operator.
	/// </summary>
	public class LeftDivide : BinaryOperator 
	{
		/// <summary>
		/// Left-Division operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LeftDivide(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			return EvaluateDivision(Left, Right, this);
		}

		/// <summary>
		/// Divides the left operand from the right one.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateDivision(IElement Left, IElement Right, ScriptNode Node)
		{
			IRingElement RE = Right as IRingElement;
			IElement Result;
			IRingElement Temp;

			if (Left is IRingElement LE && RE != null)
			{
				// TODO: Optimize in case of matrices. It's more efficient to employ a solve algorithm than to compute the inverse and the multiply.

				Temp = LE.Invert();
				if (Temp != null)
				{
					Result = RE.MultiplyLeft(Temp);
					if (Result != null)
						return Result;

					Result = Temp.MultiplyRight(RE);
					if (Result != null)
						return Result;
				}
			}

			if (Left.IsScalar)
			{
				if (Right.IsScalar)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;

					if (!LeftSet.Equals(RightSet))
					{
						if (!Expression.UpgradeField(ref Left, ref LeftSet, ref Right, ref RightSet, Node))
							throw new ScriptRuntimeException("Incompatible operands.", Node);

						LE = Left as IRingElement;
						RE = Right as IRingElement;
						if (LE != null && RE != null)
						{
							Temp = LE.Invert();
							if (Temp != null)
							{
								Result = RE.MultiplyLeft(Temp);
								if (Result != null)
									return Result;

								Result = Temp.MultiplyRight(RE);
								if (Result != null)
									return Result;
							}
						}
					}

					throw new ScriptRuntimeException("Operands cannot be divided.", Node);
				}
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluateDivision(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluateDivision(LeftChild, Right, Node));

					return Left.Encapsulate(Elements, Node);
				}
				else
				{
					ISet Set2 = Right as ISet;

					if (Left is ISet Set1 && Set2 != null)
						return new SetDifference(Set1, Set2);
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
									Elements.AddLast(EvaluateDivision(eLeft.Current, eRight.Current, Node));
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
									RightResult.AddLast(EvaluateDivision(LeftChild, RightChild, Node));

								LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
							}

							return Left.Encapsulate(LeftResult, Node);
						}
					}
				}
			}
		}

	}
}
