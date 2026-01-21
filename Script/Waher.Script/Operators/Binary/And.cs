using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Binary
{
	/// <summary>
	/// Binary And.
	/// </summary>
	public class And : BinaryDoubleOperator
	{
		/// <summary>
		/// Binary And.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public And(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
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
			ulong L = ToUInt64(Left, out bool LSigned, this);
			ulong R = ToUInt64(Right, out bool RSigned, this);

			L &= R;

			if (LSigned && RSigned)
				return new DoubleNumber((long)L);
			else
				return new DoubleNumber(L);
		}

		internal static ulong ToUInt64(double Operand, out bool Signed, ScriptNode Node)
		{
			if (Operand != Math.Floor(Operand))
				throw new OperandNonIntegerScriptException(Node);

			ulong Result;

			if (Operand < 0)
			{
				Signed = true;
				if (Operand < long.MinValue)
					throw new OperandOutOfBoundsScriptException(Node);

				Result = (ulong)(long)Operand;
			}
			else
			{
				Signed = false;
				if (Operand > ulong.MaxValue)
					throw new OperandOutOfBoundsScriptException(Node);

				Result = (ulong)Operand;
			}

			return Result;
		}
	}
}
