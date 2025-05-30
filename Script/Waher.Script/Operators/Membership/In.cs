﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// In operator
	/// </summary>
	public class In : BinaryOperator
	{
		/// <summary>
		/// In operator
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public In(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			return this.Evaluate(Left, Right);
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
			if (Right is ISet Set)
				return this.Evaluate(Left, Set);
			else if (Right is IVector Vector)
				return this.Evaluate(Left, Vector);
			else if (Right.IsScalar)
				throw new ScriptRuntimeException("Right operand in an IN operation must be a set.", this);
			else
			{
				if (Left.IsScalar)
				{
					ChunkedList<IElement> Elements = new ChunkedList<IElement>();

					foreach (IElement E in Right.ChildElements)
						Elements.Add(this.Evaluate(Left, E));

					return Right.Encapsulate(Elements, this);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						ChunkedList<IElement> Elements = new ChunkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Elements.Add(this.Evaluate(eLeft.Current, eRight.Current));
						}
						finally
						{
							eLeft.Dispose();
							eRight.Dispose();
						}

						return Left.Encapsulate(Elements, this);
					}
					else
					{
						ChunkedList<IElement> LeftResult = new ChunkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							ChunkedList<IElement> RightResult = new ChunkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.Add(this.Evaluate(LeftChild, RightChild));

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
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Left, ISet Right)
		{
			if (Right.Contains(Left))
				return BooleanValue.True;
			else
				return BooleanValue.False;
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Left, IVector Right)
		{
			foreach (IElement Element in Right.ChildElements)
			{
				if (Left.Equals(Element))
					return BooleanValue.True;
			}

			return BooleanValue.False;
		}

	}
}
