using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all ternary operators.
	/// </summary>
	public abstract class TernaryOperator : BinaryOperator
	{
		/// <summary>
		/// Middle operand.
		/// </summary>
		protected ScriptNode middle;

		/// <summary>
		/// Base class for all ternary operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Middle">Middle operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TernaryOperator(ScriptNode Left, ScriptNode Middle, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
			this.middle = Middle;
		}

		/// <summary>
		/// Middle operand.
		/// </summary>
		public ScriptNode MiddleOperand
		{
			get { return this.middle; }
		}

	}
}
