using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering
{
	/// <summary>
	/// Base Interface for all sensor-data processor nodes.
	/// </summary>
	public interface ISensorDataProcessor : IProcessorNode
	{
		/// <summary>
		/// Process a collection of sensor data fields.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the fields.</param>
		/// <param name="Fields">Fields to process.</param>
		/// <returns>Processed set of fields.</returns>
		Task<Field[]> ProcessFields(ISensor Sensor, Field[] Fields);
	}
}
