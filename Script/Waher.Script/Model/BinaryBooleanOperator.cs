using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for binary boolean operators.
	/// </summary>
	public abstract class BinaryBooleanOperator : BinaryScalarOperator
	{
		private bool? bothBool = null;

		/// <summary>
		/// Base class for binary boolean operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BinaryBooleanOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			BooleanValue BL = L as BooleanValue;
			BooleanValue BR;
			IElement Result;
			IElement R;

			if (this.bothBool.HasValue && this.bothBool.Value && !(BL is null))
			{
				bool LValue = BL.Value;
				Result = this.EvaluateOptimizedResult(LValue);
				if (!(Result is null))
					return Result;

				R = this.right.Evaluate(Variables);
				BR = R as BooleanValue;

				if (!(BR is null))
					return this.Evaluate(LValue, BR.Value);
				else
					this.bothBool = false;
			}
			else
			{
				R = this.right.Evaluate(Variables);
				BR = R as BooleanValue;

				if (!(BL is null) && !(BR is null))
				{
					if (!this.bothBool.HasValue)
						this.bothBool = true;

					return this.Evaluate(BL.Value, BR.Value);
				}
				else
				{
					this.bothBool = false;
					return this.Evaluate(L, R, Variables);
				}
			}

			return this.Evaluate(L, R, Variables);
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
			if (Left is BooleanValue BL && Right is BooleanValue BR)
				return this.Evaluate(BL.Value, BR.Value);
			else
				throw new ScriptRuntimeException("Scalar operands must be boolean values.", this);
		}

		/// <summary>
		/// Gives the operator a chance to optimize its execution if it knows the value of the left operand. This method is only called
		/// if both operands evaluated to boolean values last time the operator was evaluated.
		/// </summary>
		/// <param name="Left">Value of left operand.</param>
		/// <returns>Optimized result, if possble, or null if both operands are required.</returns>
		public abstract IElement EvaluateOptimizedResult(bool Left);

		/// <summary>
		/// Evaluates the boolean operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(bool Left, bool Right);

	}
}
