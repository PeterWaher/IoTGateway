using System;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields
{
	/// <summary>
	/// Abstract base class for decision tree statement nodes.
	/// </summary>
	public abstract class DecisionTreeStatement : ProcessorNode, IDecisionTreeStatement
	{
		/// <summary>
		/// Abstract base class for decision tree statement nodes.
		/// </summary>
		public DecisionTreeStatement()
			: base()
		{
			this.NodeId = Guid.NewGuid().ToString();
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
			return Task.FromResult(Parent is IDecisionTreeStatements);
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public abstract Task<Field[]> ProcessField(ISensor Sensor, Field Field);
	}
}
