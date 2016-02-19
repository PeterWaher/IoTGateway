using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Elements.Interfaces;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for binary vector operators.
	/// </summary>
	public abstract class BinaryVectorOperator : BinaryOperator
	{
		/// <summary>
		/// Base class for binary vector operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public BinaryVectorOperator(ScriptNode Left, ScriptNode Right, int Start, int Length)
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

			return this.Evaluate(Left, Right);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Left, IElement Right)
		{
			IVector VL;
			IVector VR;

			if ((VL = Left as IVector) != null)
			{
				if ((VR = Right as IVector) != null)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;
					UpgradeBehaviour UpgradeBehaviour;

					if (!LeftSet.Equals(RightSet) && (UpgradeBehaviour = this.ScalarUpgradeBehaviour) != UpgradeBehaviour.DifferentTypesOk)
					{
						if (!Expression.Upgrade(ref Left, ref LeftSet, ref Right, ref RightSet, this))
						{
							if (UpgradeBehaviour == UpgradeBehaviour.SameTypeRequired)
								throw new ScriptRuntimeException("Incompatible operands.", this);
						}
					}

					return this.EvaluateVector(VL, VR);
				}
				else
				{
					LinkedList<IElement> Result = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Result.AddLast(this.Evaluate(Left, RightChild));

					return Right.Encapsulate(Result, this);
				}
			}
			else
			{
				if ((VR = Right as IVector) != null)
				{
					LinkedList<IElement> Result = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Result.AddLast(this.Evaluate(LeftChild, Right));

					return Left.Encapsulate(Result, this);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						LinkedList<IElement> Result = new LinkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Result.AddLast(this.Evaluate(eLeft.Current, eRight.Current));
						}
						finally
						{
							eLeft.Dispose();
							eRight.Dispose();
						}

						return Left.Encapsulate(Result, this);
					}
					else
					{
						LinkedList<IElement> LeftResult = new LinkedList<IElement>();

						foreach (IElement LeftChild in Left.ChildElements)
						{
							LinkedList<IElement> RightResult = new LinkedList<IElement>();

							foreach (IElement RightChild in Right.ChildElements)
								RightResult.AddLast(this.Evaluate(LeftChild, RightChild));

							LeftResult.AddLast(Right.Encapsulate(RightResult, this));
						}

						return Left.Encapsulate(LeftResult, this);
					}
				}
			}
		}

		/// <summary>
		/// Evaluates the operator on vector operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public abstract IElement EvaluateVector(IVector Left, IVector Right);

		/// <summary>
		/// How scalar operands of different types are to be treated. By default, scalar operands are required to be of the same type.
		/// </summary>
		public virtual UpgradeBehaviour ScalarUpgradeBehaviour
		{
			get { return UpgradeBehaviour.SameTypeRequired; }
		}
	}
}
