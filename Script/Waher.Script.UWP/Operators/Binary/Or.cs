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
	/// Binary Or.
	/// </summary>
	public class Or : BinaryDoubleOperator 
	{
		/// <summary>
		/// Binary Or.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Or(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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

			L |= R;

			if (LSigned && RSigned)
				return new DoubleNumber((long)L);
			else
				return new DoubleNumber(L);
		}
	}
}
