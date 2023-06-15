using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
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
		/// <param name="Expression">Expression containing script.</param>
		public BinaryDualBoolDoubleOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			IElement R = null;
			IElement Result;
			BooleanValue BL = L as BooleanValue;
			BooleanValue BR;

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

			if (R is null)
			{
				if (!(BL is null) && !this.bothBool.HasValue)
				{
					try
					{
						R = this.right.Evaluate(Variables);
						BR = R as BooleanValue;

						if (!(BR is null))
						{
							this.bothBool = true;
							return this.Evaluate(BL.Value, BR.Value);
						}
						else
							this.bothBool = false;
					}
					catch (Exception ex)
					{
						Result = this.EvaluateOptimizedResult(BL.Value);
						if (Result is null)
							ExceptionDispatchInfo.Capture(ex).Throw();
						else
						{
							this.bothBool = true;
							return Result;
						}
					}
				}
				else
					R = this.right.Evaluate(Variables);
			}

			if (this.bothDouble.HasValue)
			{
				if (this.bothDouble.Value)
				{
					if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
						return this.Evaluate(DL, DR);
					else
						this.bothDouble = false;
				}
			}
			else
			{
				if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
				{
					this.bothDouble = true;
					return this.Evaluate(DL, DR);
				}
				else
					this.bothDouble = false;
			}

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
			IElement R = null;
			IElement Result;
			BooleanValue BL = L as BooleanValue;
			BooleanValue BR;

			if (this.bothBool.HasValue && this.bothBool.Value && !(BL is null))
			{
				bool LValue = BL.Value;
				Result = await this.EvaluateOptimizedResultAsync(LValue);
				if (!(Result is null))
					return Result;

				R = await this.right.EvaluateAsync(Variables);
				BR = R as BooleanValue;

				if (!(BR is null))
					return await this.EvaluateAsync(LValue, BR.Value);
				else
					this.bothBool = false;
			}

			if (R is null)
			{
				if (!(BL is null) && !this.bothBool.HasValue)
				{
					try
					{
						R = await this.right.EvaluateAsync(Variables);
						BR = R as BooleanValue;

						if (!(BR is null))
						{
							this.bothBool = true;
							return await this.EvaluateAsync(BL.Value, BR.Value);
						}
						else
							this.bothBool = false;
					}
					catch (Exception ex)
					{
						Result = await this.EvaluateOptimizedResultAsync(BL.Value);
						if (Result is null)
							ExceptionDispatchInfo.Capture(ex).Throw();
						else
						{
							this.bothBool = true;
							return Result;
						}
					}
				}
				else
					R = await this.right.EvaluateAsync(Variables);
			}

			if (this.bothDouble.HasValue)
			{
				if (this.bothDouble.Value)
				{
					if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
						return await this.EvaluateAsync(DL, DR);
					else
						this.bothDouble = false;
				}
			}
			else
			{
				if (L.AssociatedObjectValue is double DL && R.AssociatedObjectValue is double DR)
				{
					this.bothDouble = true;
					return await this.EvaluateAsync(DL, DR);
				}
				else
					this.bothDouble = false;
			}

			return await this.EvaluateAsync(L, R, Variables);
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
			if (Left.AssociatedObjectValue is bool BL &&
				Right.AssociatedObjectValue is bool BR)
			{
				return this.Evaluate(BL, BR);
			}
			else
			{
				if (!(Left.AssociatedObjectValue is double l) &&
					!Expression.TryConvert(Left.AssociatedObjectValue, out l))
				{
					throw new ScriptRuntimeException("Scalar operands must be double values.", this);
				}

				if (!(Right.AssociatedObjectValue is double r) &&
					!Expression.TryConvert(Right.AssociatedObjectValue, out r))
				{
					throw new ScriptRuntimeException("Scalar operands must be double values.", this);
				}

				return this.Evaluate(l, r);
			}
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Left, IElement Right, Variables Variables)
		{
			if (Left.AssociatedObjectValue is bool BL &&
				Right.AssociatedObjectValue is bool BR)
			{
				return this.EvaluateAsync(BL, BR);
			}
			else
			{
				if (!(Left.AssociatedObjectValue is double l) &&
					!Expression.TryConvert(Left.AssociatedObjectValue, out l))
				{
					throw new ScriptRuntimeException("Scalar operands must be double values.", this);
				}

				if (!(Right.AssociatedObjectValue is double r) &&
					!Expression.TryConvert(Right.AssociatedObjectValue, out r))
				{
					throw new ScriptRuntimeException("Scalar operands must be double values.", this);
				}

				return this.EvaluateAsync(l, r);
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

		/// <summary>
		/// Gives the operator a chance to optimize its execution if it knows the value of the left operand. This method is only called
		/// if both operands evaluated to boolean values last time the operator was evaluated.
		/// </summary>
		/// <param name="Left">Value of left operand.</param>
		/// <returns>Optimized result, if possble, or null if both operands are required.</returns>
		public virtual Task<IElement> EvaluateOptimizedResultAsync(bool Left)
		{
			return Task.FromResult<IElement>(this.EvaluateOptimizedResult(Left));
		}

		/// <summary>
		/// Evaluates the boolean operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateAsync(bool Left, bool Right)
		{
			return Task.FromResult<IElement>(this.Evaluate(Left, Right));
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateAsync(double Left, double Right)
		{
			return Task.FromResult<IElement>(this.Evaluate(Left, Right));
		}

	}
}
