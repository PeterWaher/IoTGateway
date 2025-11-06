using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Output.Metering
{
	/// <summary>
	/// Base Interface for all sensor-data output nodes.
	/// </summary>
	public interface ISensorDataOutput : IOutputNode
	{
		/// <summary>
		/// Outputs a collection of sensor data fields.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the fields.</param>
		/// <param name="Fields">Fields to output.</param>
		Task OutputFields(ISensor Sensor, Field[] Fields);
	}
}
