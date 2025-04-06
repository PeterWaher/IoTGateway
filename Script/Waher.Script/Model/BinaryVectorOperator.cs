using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;

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
		/// <param name="Expression">Expression containing script.</param>
		public BinaryVectorOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			return this.Evaluate(Left, Right, Variables);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement Left = await this.left.EvaluateAsync(Variables);
			IElement Right = await this.right.EvaluateAsync(Variables);

			return await this.EvaluateAsync(Left, Right, Variables);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Left, IElement Right, Variables Variables)
		{
			if (Left is IVector VL)
			{
				if (Right is IVector VR)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;
					UpgradeBehaviour UpgradeBehaviour;

					if (!LeftSet.Equals(RightSet) && (UpgradeBehaviour = this.ScalarUpgradeBehaviour) != UpgradeBehaviour.DifferentTypesOk)
					{
						if (!Expression.UpgradeField(ref Left, ref LeftSet, ref Right, ref RightSet))
						{
							if (UpgradeBehaviour == UpgradeBehaviour.SameTypeRequired)
								throw new ScriptRuntimeException("Incompatible operands.", this);
						}
					}

					return this.EvaluateVector(VL, VR, Variables);
				}
				else
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Result.Add(this.Evaluate(Left, RightChild, Variables));

					return Right.Encapsulate(Result, this);
				}
			}
			else
			{
				if (Right is IVector)
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Result.Add(this.Evaluate(LeftChild, Right, Variables));

					return Left.Encapsulate(Result, this);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						ChunkedList<IElement> Result = new ChunkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Result.Add(this.Evaluate(eLeft.Current, eRight.Current, Variables));
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
						ChunkedList<IElement> LeftResult = new ChunkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							ChunkedList<IElement> RightResult = new ChunkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.Add(this.Evaluate(LeftChild, RightChild, Variables));

							LeftResult.Add(Right.Encapsulate(RightResult, this));
						}

						return Left.Encapsulate(LeftResult, this);
					}
				}
			}
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public virtual async Task<IElement> EvaluateAsync(IElement Left, IElement Right, Variables Variables)
		{
			if (Left is IVector VL)
			{
				if (Right is IVector VR)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;
					UpgradeBehaviour UpgradeBehaviour;

					if (!LeftSet.Equals(RightSet) && (UpgradeBehaviour = this.ScalarUpgradeBehaviour) != UpgradeBehaviour.DifferentTypesOk)
					{
						if (!Expression.UpgradeField(ref Left, ref LeftSet, ref Right, ref RightSet))
						{
							if (UpgradeBehaviour == UpgradeBehaviour.SameTypeRequired)
								throw new ScriptRuntimeException("Incompatible operands.", this);
						}
					}

					return await this.EvaluateVectorAsync(VL, VR, Variables);
				}
				else
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Result.Add(await this.EvaluateAsync(Left, RightChild, Variables));

					return Right.Encapsulate(Result, this);
				}
			}
			else
			{
				if (Right is IVector)
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Result.Add(await this.EvaluateAsync(LeftChild, Right, Variables));

					return Left.Encapsulate(Result, this);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						ChunkedList<IElement> Result = new ChunkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Result.Add(await this.EvaluateAsync(eLeft.Current, eRight.Current, Variables));
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
						ChunkedList<IElement> LeftResult = new ChunkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							ChunkedList<IElement> RightResult = new ChunkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.Add(await this.EvaluateAsync(LeftChild, RightChild, Variables));

							LeftResult.Add(Right.Encapsulate(RightResult, this));
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
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public abstract IElement EvaluateVector(IVector Left, IVector Right, Variables Variables);

		/// <summary>
		/// Evaluates the operator on vector operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateVectorAsync(IVector Left, IVector Right, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateVector(Left, Right, Variables));
		}

		/// <summary>
		/// How scalar operands of different types are to be treated. By default, scalar operands are required to be of the same type.
		/// </summary>
		public virtual UpgradeBehaviour ScalarUpgradeBehaviour => UpgradeBehaviour.SameTypeRequired;
	}
}
