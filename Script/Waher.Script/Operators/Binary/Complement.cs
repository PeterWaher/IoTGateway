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
	/// Complement operator.
	/// </summary>
	public class Complement : UnaryDoubleOperator
	{
		/// <summary>
		/// Complement operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Complement(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Operand)
		{
			if (Operand != Math.Floor(Operand))
				throw new ScriptRuntimeException("Operands must be integer values.", this);

			if (Operand < 0)
			{
				if (Operand < long.MinValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				return new DoubleNumber(~(long)Operand);
			}
			else
			{
				if (Operand > ulong.MaxValue)
					throw new ScriptRuntimeException("Operand out of bounds.", this);

				return new DoubleNumber(~(ulong)Operand);
			}
		}
	}
}
