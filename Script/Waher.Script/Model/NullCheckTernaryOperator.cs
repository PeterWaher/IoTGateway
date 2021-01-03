using System;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all unary operators performing operand null checks.
	/// </summary>
	public abstract class NullCheckTernaryOperator : TernaryOperator 
	{
		/// <summary>
		/// If null should be returned if operand is null.
		/// </summary>
		protected readonly bool nullCheck;

		/// <summary>
		/// Base class for all unary operators performing operand null checks.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Middle">Middle operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NullCheckTernaryOperator(ScriptNode Left, ScriptNode Middle, ScriptNode Right, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Left, Middle, Right, Start, Length, Expression)
		{
			this.nullCheck = NullCheck;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is NullCheckTernaryOperator O &&
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
