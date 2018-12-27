using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all unary scalar operators performing operand null checks.
	/// </summary>
	public abstract class NullCheckUnaryScalarOperator : UnaryScalarOperator
	{
		/// <summary>
		/// If null should be returned if operand is null.
		/// </summary>
		protected readonly bool nullCheck;

		/// <summary>
		/// Base class for all unary scalar operators performing operand null checks.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NullCheckUnaryScalarOperator(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.nullCheck = NullCheck;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is NullCheckUnaryScalarOperator O &&
				this.nullCheck.Equals(O.nullCheck) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.nullCheck.GetHashCode();
			return Result;
		}

	}
}
