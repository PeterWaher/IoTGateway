using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// Abstract base class for joins of two source definitions.
	/// </summary>
	public abstract class Join : SourceDefinition
	{
		private SourceDefinition left;
		private SourceDefinition right;
		private ScriptNode conditions;

		/// <summary>
		/// Abstract base class for joins of two source definitions.
		/// </summary>
		/// <param name="Left">Left source definition.</param>
		/// <param name="Right">Right source definition.</param>
		/// <param name="Conditions">Join conditions.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(SourceDefinition Left, SourceDefinition Right, ScriptNode Conditions, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.left = Left;
			this.right = Right;
			this.conditions = Conditions;
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
				if (!(this.left?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			ScriptNode Node = this.left;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.left)
			{
				if (Node is SourceDefinition Left2)
					this.left = Left2;
				else
					return false;
			}

			Node = this.right;
			if (!(Node is null) && !Callback(ref Node, State))
				return false;

			if (Node != this.right)
			{
				if (Node is SourceDefinition Right2)
					this.right = Right2;
				else
					return false;
			}

			if (!(this.conditions is null) && !Callback(ref this.conditions, State))
				return false;

			if (!DepthFirst)
			{
				if (!(this.left?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, DepthFirst) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is Join O &&
				AreEqual(this.left, O.left) &&
				AreEqual(this.right, O.right) &&
				AreEqual(this.conditions, O.conditions) &&
				base.Equals(obj));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.left);
			Result ^= Result << 5 ^ GetHashCode(this.right);
			Result ^= Result << 5 ^ GetHashCode(this.conditions);

			return Result;
		}

	}
}
