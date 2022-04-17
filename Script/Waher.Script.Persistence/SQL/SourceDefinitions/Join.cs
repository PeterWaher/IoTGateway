using System;
using Waher.Script.Model;

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
			this.left?.SetParent(this);

			this.right = Right;
			this.right?.SetParent(this);

			this.conditions = Conditions;
			this.conditions?.SetParent(this);
		}

		/// <summary>
		/// Left source
		/// </summary>
		public SourceDefinition Left => this.left;

		/// <summary>
		/// Right source
		/// </summary>
		public SourceDefinition Right => this.right;

		/// <summary>
		/// Conditions linking the two sources.
		/// </summary>
		public ScriptNode Conditions => this.conditions;

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			ScriptNode NewNode;
			bool b;

			if (!(this.left is null))
			{
				b = !Callback(this.left, out NewNode, State);
				if (!(NewNode is null) && NewNode is SourceDefinition Left2)
				{
					this.left = Left2;
					this.left.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.left.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.right is null))
			{
				b = !Callback(this.right, out NewNode, State);
				if (!(NewNode is null) && NewNode is SourceDefinition Right2)
				{
					this.right = Right2;
					this.right.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.right.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.conditions is null))
			{
				b = !Callback(this.conditions, out NewNode, State);
				if (!(NewNode is null))
				{
					this.conditions = NewNode;
					this.conditions.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.conditions.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.conditions?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is Join O &&
				AreEqual(this.left, O.left) &&
				AreEqual(this.right, O.right) &&
				AreEqual(this.conditions, O.conditions) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
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
