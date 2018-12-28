using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Subtraction operator.
	/// </summary>
	public class Subtract : BinaryOperator, IDifferentiable
	{
		/// <summary>
		/// Subtraction operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Subtract(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			return EvaluateSubtraction(Left, Right, this);
		}

		/// <summary>
		/// Subtracts the right operand from the left one.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateSubtraction(IElement Left, IElement Right, ScriptNode Node)
		{
			IGroupElement LE = Left as IGroupElement;
			IGroupElement RE = Right as IGroupElement;
			IElement Result;
			IGroupElement Temp;

			if (!(LE is null) && !(RE is null))
			{
				Temp = RE.Negate();
				if (!(Temp is null))
				{
					Result = LE.AddRight(Temp);
					if (!(Result is null))
						return Result;

					Result = Temp.AddLeft(LE);
					if (!(Result is null))
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

						LE = Left as IGroupElement;
						RE = Right as IGroupElement;
						if (!(LE is null) && !(RE is null))
						{
							Temp = RE.Negate();
							if (!(Temp is null))
							{
								Result = LE.AddRight(Temp);
								if (!(Result is null))
									return Result;

								Result = Temp.AddLeft(LE);
								if (!(Result is null))
									return Result;
							}
						}
					}

					throw new ScriptRuntimeException("Operands cannot be subtracted.", Node);
				}
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluateSubtraction(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluateSubtraction(LeftChild, Right, Node));

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
								Elements.AddLast(EvaluateSubtraction(eLeft.Current, eRight.Current, Node));
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
								RightResult.AddLast(EvaluateSubtraction(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (this.left is IDifferentiable Left &&
				this.right is IDifferentiable Right)
			{
				return new Subtract(
					Left.Differentiate(VariableName, Variables),
					Right.Differentiate(VariableName, Variables),
					this.Start, this.Length, this.Expression);
			}
			else
				throw new ScriptRuntimeException("Terms not differentiable.", this);
		}

	}
}
