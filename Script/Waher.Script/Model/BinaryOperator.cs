using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all binary operators.
	/// </summary>
	public abstract class BinaryOperator : ScriptNode
	{
		/// <summary>
		/// Left operand.
		/// </summary>
		protected ScriptNode left;

		/// <summary>
		/// Right operand.
		/// </summary>
		protected ScriptNode right;

		/// <summary>
		/// Base class for all binary operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public BinaryOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.left = Left;
			this.right = Right;
		}

		/// <summary>
		/// Left operand.
		/// </summary>
		public ScriptNode LeftOperand
		{
			get { return this.left; }
		}

		/// <summary>
		/// Right operand.
		/// </summary>
		public ScriptNode RightOperand
		{
			get { return this.right; }
		}

	}
}
