using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Binary
{
	/// <summary>
	/// Shift right operator.
	/// </summary>
	public class ShiftRight : BinaryDoubleOperator 
	{
		/// <summary>
		/// Shift right operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ShiftRight(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Left">Left value.</param>
		/// <param name="Right">Right value.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Left, double Right)
		{
			sbyte R;

			if (Left != Math.Floor(Left) || Right != Math.Floor(Right))
				throw new OperandNonIntegerScriptException(this);

			if (Right < sbyte.MinValue || Right > sbyte.MaxValue)
				throw new OperandOutOfBoundsScriptException(this);

			R = (sbyte)Right;

			if (Left < 0)
			{
				if (Left < long.MinValue)
					throw new OperandOutOfBoundsScriptException(this);

				return new DoubleNumber(((long)Left) >> R);
			}
			else
			{
				if (Left > ulong.MaxValue)
					throw new OperandOutOfBoundsScriptException(this);

				return new DoubleNumber(((ulong)Left) >> R);
			}
		}
	}
}
