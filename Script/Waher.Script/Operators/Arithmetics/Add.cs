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
	/// Addition operator.
	/// </summary>
	public class Add : BinaryOperator 
	{
		/// <summary>
		/// Addition operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Add(ScriptNode Left, ScriptNode Right, int Start, int Length)
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

			return EvaluateAddition(Left, Right, this);
		}

		/// <summary>
		/// Adds two operands.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateAddition(IElement Left, IElement Right, ScriptNode Node)
		{
			ISemiGroupElement LE = Left as ISemiGroupElement;
			ISemiGroupElement RE = Right as ISemiGroupElement;
			IElement Result;

			if (LE != null && RE != null)
			{
				Result = LE.AddRight(RE);
				if (Result != null)
					return Result;

				Result = RE.AddLeft(LE);
				if (Result != null)
					return Result;
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

						LE = Left as ISemiGroupElement;
						RE = Right as ISemiGroupElement;
						if (LE != null && RE != null)
						{
							Result = LE.AddRight(RE);
							if (Result != null)
								return Result;

							Result = RE.AddLeft(LE);
							if (Result != null)
								return Result;
						}
					}

					throw new ScriptRuntimeException("Operands cannot be added.", Node);
				}
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluateAddition(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluateAddition(LeftChild, Right, Node));

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
								Elements.AddLast(EvaluateAddition(eLeft.Current, eRight.Current, Node));
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
								RightResult.AddLast(EvaluateAddition(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

	}
}
