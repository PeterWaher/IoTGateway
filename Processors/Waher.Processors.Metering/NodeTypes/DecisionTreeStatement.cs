using System;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes
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
