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

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!this.left.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.middle.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.right.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			if (!Callback(ref this.left, State))
				return false;

			if (!Callback(ref this.middle, State))
				return false;

			if (!Callback(ref this.right, State))
				return false;

			if (!DepthFirst)
			{
				if (!this.left.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.middle.ForAllChildNodes(Callback, State, DepthFirst))
					return false;

				if (!this.right.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is TernaryOperator O &&
				this.middle.Equals(O.middle) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.middle.GetHashCode();
			return Result;
		}

	}
}
