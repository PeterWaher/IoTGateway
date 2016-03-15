using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all quaternary operators.
	/// </summary>
	public abstract class QuaternaryOperator : TernaryOperator
	{
		/// <summary>
		/// Second Middle operand.
		/// </summary>
		protected ScriptNode middle2;

		/// <summary>
		/// Base class for all ternary operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Middle">Middle operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public QuaternaryOperator(ScriptNode Left, ScriptNode Middle1, ScriptNode Middle2, ScriptNode Right, int Start, int Length)
			: base(Left, Middle1, Right, Start, Length)
		{
			this.middle2 = Middle2;
		}

		/// <summary>
		/// Second middle operand.
		/// </summary>
		public ScriptNode Middle2Operand
		{
			get { return this.middle2; }
		}

	}
}
