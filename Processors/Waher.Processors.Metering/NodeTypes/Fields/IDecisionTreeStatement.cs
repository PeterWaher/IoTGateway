using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields
{
	/// <summary>
	/// Interface for decision tree staement nodes.
	/// </summary>
	public interface IDecisionTreeStatement : IProcessorNode
	{
		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		Task<Field[]> ProcessField(ISensor Sensor, Field Field);
	}
}
