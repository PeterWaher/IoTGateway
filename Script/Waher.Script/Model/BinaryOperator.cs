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
		/// <param name="Expression">Expression containing script.</param>
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

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public virtual string DefaultVariableName
		{
			get
			{
				if (this.left is IDifferentiable Left &&
					this.right is IDifferentiable Right)
				{
					string s = Left.DefaultVariableName;
					if (s is null)
						return null;
					else if (s == Right.DefaultVariableName)
						return s;
					else
						return null;
				}
				else
					return null;
			}
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

				if (!this.right.ForAllChildNodes(Callback, State, DepthFirst))
					return false;
			}

			if (!Callback(ref this.left, State))
				return false;

			if (!Callback(ref this.right, State))
				return false;

			if (!DepthFirst)
			{
				if (!this.left.ForAllChildNodes(Callback, State, DepthFirst))
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
			return obj is BinaryOperator O &&
				this.left.Equals(O.left) &&
				this.right.Equals(O.right) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.left.GetHashCode();
			Result ^= Result << 5 ^ this.right.GetHashCode();
			return Result;
		}
	}
}
