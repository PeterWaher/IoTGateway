using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// Binary null check operator.
	/// </summary>
	public class NullCheck : BinaryScalarOperator
	{
		/// <summary>
		/// Binary null check operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NullCheck(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			IElement Left;

			try
			{
				Left = this.left.Evaluate(Variables);
			}
			catch (Exception)
			{
				return this.right.Evaluate(Variables);
			}

			if (!(Left.AssociatedObjectValue is null))
				return Left;

			IElement Right = this.right.Evaluate(Variables);

			return this.Evaluate(Left, Right, Variables);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Left, IElement Right, Variables Variables)
		{
			if (Left.AssociatedObjectValue is null)
				return Right;
			else
				return Left;
		}

		/// <summary>
		/// How scalar operands of different types are to be treated. By default, scalar operands are required to be of the same type.
		/// </summary>
		public override UpgradeBehaviour ScalarUpgradeBehaviour => UpgradeBehaviour.DifferentTypesOk;
	}
}
