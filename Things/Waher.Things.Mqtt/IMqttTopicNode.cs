using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// Interface for MQTT Topic nodes.
	/// </summary>
	public interface IMqttTopicNode : IMeteringNode, IProcessingSupport<MqttTopicRepresentation>
	{
		/// <summary>
		/// Local Topic segment
		/// </summary>
		string LocalTopic { get; }

		/// <summary>
		/// Gets the full topic string.
		/// </summary>
		Task<string> GetFullTopic();

		/// <summary>
		/// Creates a new node of the same type.
		/// </summary>
		/// <param name="Topic">MQTT Topic being processed.</param>
		/// <returns>New node instance.</returns>
		Task<IMqttTopicNode> CreateNew(MqttTopicRepresentation Topic);

		/// <summary>
		/// Gets the default data object, if any.
		/// </summary>
		/// <returns>Default data object, if one exists, or null otherwise.</returns>
		Task<IMqttData> GetDefaultDataObject();
	}
}
