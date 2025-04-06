using System;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for binary element-wise double operators.
	/// </summary>
	public abstract class BinaryElementWiseDoubleOperator : BinaryDoubleOperator
	{
		/// <summary>
		/// Base class for binary element-wise double operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BinaryElementWiseDoubleOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			IElement L = this.left.Evaluate(Variables);
			IElement R = this.right.Evaluate(Variables);

			if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
				return this.Evaluate(DL, DR);
			else
				return this.Evaluate(L, R, Variables);
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

			IElement L = await this.left.EvaluateAsync(Variables);
			IElement R = await this.right.EvaluateAsync(Variables);

			if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
				return await this.EvaluateAsync(DL, DR);
			else
				return await this.EvaluateAsync(L, R, Variables);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(IElement Left, IElement Right, Variables Variables)
		{
			ISet LS = Left.AssociatedSet;
			ISet RS = Right.AssociatedSet;

			if (!LS.Equals(RS))
			{
				bool b;

				try
				{
					b = LS is IRightModule RM && RM.ScalarRing.Contains(Right);
				}
				catch (Exception)
				{
					b = false;
				}

				if (b)
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Result.Add(this.Evaluate(LeftChild, Right, Variables));

					return Left.Encapsulate(Result, this);
				}

				try
				{
					b = RS is ILeftModule LM && LM.ScalarRing.Contains(Left);
				}
				catch (Exception)
				{
					b = false;
				}

				if (b)
				{
					ChunkedList<IElement> Result = new ChunkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Result.Add(this.Evaluate(Left, RightChild, Variables));

					return Right.Encapsulate(Result, this);
				}
			}

			return base.Evaluate(Left, Right, Variables);
		}

	}
}
