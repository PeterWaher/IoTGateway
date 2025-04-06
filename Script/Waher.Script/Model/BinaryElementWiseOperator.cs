using System;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for binary element-wise operators.
	/// </summary>
	public abstract class BinaryElementWiseOperator : BinaryScalarOperator
	{
		/// <summary>
		/// Base class for binary element-wise operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BinaryElementWiseOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
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

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override async Task<IElement> EvaluateAsync(IElement Left, IElement Right, Variables Variables)
		{
			if (!this.IsAsynchronous)
				return this.Evaluate(Left, Right, Variables);

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
						Result.Add(await this.EvaluateAsync(LeftChild, Right, Variables));

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
						Result.Add(await this.EvaluateAsync(Left, RightChild, Variables));

					return Right.Encapsulate(Result, this);
				}
			}

			return await base.EvaluateAsync(Left, Right, Variables);
		}
	}
}
