using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all unary operators performing operand null checks.
	/// </summary>
	public abstract class NullCheckUnaryOperator : UnaryOperator 
	{
		/// <summary>
		/// If null should be returned if operand is null.
		/// </summary>
		protected readonly bool nullCheck;

		/// <summary>
		/// Base class for all unary operators performing operand null checks.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NullCheckUnaryOperator(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.nullCheck = NullCheck;
		}

	}
}
