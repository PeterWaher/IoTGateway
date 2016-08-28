using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Dual
{
	/// <summary>
	/// Unspecified Exclusive Xor.
	/// </summary>
	public class Xor : BinaryDualBoolDoubleOperator 
	{
		/// <summary>
		/// Unspecified Exclusive Or.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Xor(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gives the operator a chance to optimize its execution if it knows the value of the left operand. This method is only called
		/// if both operands evaluated to boolean values last time the operator was evaluated.
		/// </summary>
		/// <param name="Left">Value of left operand.</param>
		/// <returns>Optimized result, if possble, or null if both operands are required.</returns>
		public override IElement EvaluateOptimizedResult(bool Left)
		{
			return null;
		}

		/// <summary>
		/// Evaluates the boolean operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(bool Left, bool Right)
		{
			if (Left ^ Right)
				return BooleanValue.True;
			else
				return BooleanValue.False;
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Left, double Right)
		{
			ulong L, R;
			bool LSigned;
			bool RSigned;

			if (Left != Math.Floor(Left) || Right != Math.Floor(Right))
				throw new ScriptRuntimeException("Operands must be integer values.", this);

			if (Left < 0)
			{
				LSigned = true;
				if (Left < long.MinValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				L = (ulong)((long)Left);
			}
			else
			{
				LSigned = false;
				if (Left > ulong.MaxValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				L = (ulong)Left;
			}

			if (Right < 0)
			{
				RSigned = true;
				if (Right < long.MinValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				R = (ulong)((long)Right);
			}
			else
			{
				RSigned = false;
				if (Right > ulong.MaxValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				R = (ulong)Right;
			}

			L ^= R;

			if (LSigned && RSigned)
				return new DoubleNumber((long)L);
			else
				return new DoubleNumber(L);
		}
	}
}
