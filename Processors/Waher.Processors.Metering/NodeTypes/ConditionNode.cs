using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes
{
	/// <summary>
	/// Abstract base class of condition nodes.
	/// </summary>
	public abstract class ConditionNode : DecisionTreeStatements, IConditionNode
	{
		/// <summary>
		/// Condition on field type.
		/// </summary>
		public ConditionNode()
			: base()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(20, "Label:", 10)]
		[Page(21, "Processor", 0)]
		[ToolTip(22, "Label presenting the node in the decision tree.")]
		[Required]
		public string Label { get; set; }

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public override string LocalId => string.IsNullOrEmpty(this.Label) ? this.NodeId : this.Label;

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		public override string LogId => this.LocalId;

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Conditional);
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public abstract Task<bool> AppliesTo(Field Field);
	}
}
