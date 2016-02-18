using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Binary
{
	/// <summary>
	/// Shift left operator.
	/// </summary>
	public class ShiftLeft : BinaryDoubleOperator
	{
		/// <summary>
		/// Shift left operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ShiftLeft(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
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
				throw new ScriptRuntimeException("Operands must be integer values.", this);

			if (Right < sbyte.MinValue || Right > sbyte.MaxValue)
				throw new ScriptRuntimeException("Operand out of bounds.", this);

			R = (sbyte)Right;

			if (Left < 0)
			{
				if (Left < long.MinValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				return new DoubleNumber(((long)Left) << R);
			}
			else
			{
				if (Left > ulong.MaxValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				return new DoubleNumber(((ulong)Left) << R);
			}
		}
	}
}
