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
	/// Base class for binary dual double/bool operators.
	/// </summary>
	public abstract class BinaryDualBoolDoubleOperator : BinaryScalarOperator
	{
		private bool? bothBool = null;
		private bool? bothDouble = null;

		/// <summary>
		/// Base class for binary dual double/bool operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public BinaryDualBoolDoubleOperator(ScriptNode Left, ScriptNode Right, int Start, int Length)
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
			IElement L = this.left.Evaluate(Variables);
			IElement R = null;

			if (this.bothDouble.HasValue && this.bothDouble.Value)
			{
				DoubleNumber DL = L as DoubleNumber;
				DoubleNumber DR = R as DoubleNumber;

				if (DL != null && DR != null)
					return this.Evaluate(DL.Value, DR.Value);
				else
					this.bothDouble = false;
			}

			BooleanValue BL = L as BooleanValue;
			BooleanValue BR;
			IElement Result;

			if (this.bothBool.HasValue && this.bothBool.Value && BL != null)
			{
				bool LValue = BL.Value;
				Result = this.EvaluateOptimizedResult(LValue);
				if (Result != null)
					return Result;

				if (R == null)
					R = this.right.Evaluate(Variables);

				BR = R as BooleanValue;

				if (BR != null)
					return this.Evaluate(LValue, BR.Value);
				else
					this.bothBool = false;
			}
			else
			{
				if (R == null)
					R = this.right.Evaluate(Variables);

				BR = R as BooleanValue;

				if (BL != null && BR != null)
				{
					if (!this.bothBool.HasValue)
						this.bothBool = true;

					return this.Evaluate(BL.Value, BR.Value);
				}
				else
				{
					this.bothBool = false;

					DoubleNumber DL = L as DoubleNumber;
					DoubleNumber DR = R as DoubleNumber;

					if (DL != null && DR != null)
					{
						if (!this.bothDouble.HasValue)
							this.bothDouble = true;

						return this.Evaluate(DL.Value, DR.Value);
					}
					else
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
			BooleanValue BL = Left as BooleanValue;
			BooleanValue BR = Right as BooleanValue;

			if (BL != null && BR != null)
				return this.Evaluate(BL.Value, BR.Value);
			else
			{
				DoubleNumber DL = Left as DoubleNumber;
				DoubleNumber DR = Right as DoubleNumber;

				if (DL != null && DR != null)
					return this.Evaluate(DL.Value, DR.Value);
				else
					throw new ScriptRuntimeException("Scalar operands must be both boolean or double values.", this);
			}
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

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(double Left, double Right);

	}
}
