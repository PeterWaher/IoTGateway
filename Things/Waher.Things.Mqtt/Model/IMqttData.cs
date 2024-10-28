using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;
using Waher.Runtime.Inventory;

namespace Waher.Things.Mqtt.Model
{
	/// <summary>
	/// Results from processing an incoming message.
	/// </summary>
	public enum DataProcessingResult
	{
		/// <summary>
		/// Data was incompatible with selected processing algorithm.
		/// </summary>
		Incompatible,

		/// <summary>
		/// Data has been processed.
		/// </summary>
		Processed,

		/// <summary>
		/// Data has been processed, and new momentary values have been detected.
		/// </summary>
		ProcessedNewMomentaryValues
	}

	/// <summary>
	/// Interface for MQTT Data encapsulations
	/// </summary>
	public interface IMqttData : IProcessingSupport<MqttContent>
	{
		/// <summary>
		/// Timestamp of data reception.
		/// </summary>
		DateTime Timestamp { get; }

		/// <summary>
		/// Quality of Service used
		/// </summary>
		MqttQualityOfService QoS { get; }

		/// <summary>
		/// Topic used
		/// </summary>
		MqttTopic Topic { get; }

		/// <summary>
		/// If retain flag was set.
		/// </summary>
		bool Retain { get; }

		/// <summary>
		/// If data can be controlled (written)
		/// </summary>
		bool IsControllable { get; }

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		/// <returns>Data processing result</returns>
		Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content);

		/// <summary>
		/// Type name representing data.
		/// </summary>
		Task<string> GetTypeName(Language Language);

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last);

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		void SnifferOutput(ISniffable Output);

		/// <summary>
		/// Outputs information to sniffer.
		/// </summary>
		void Information(ISniffable Output, string Info);

		/// <summary>
		/// Reports fields during an active readout.
		/// </summary>
		void Add(List<Field> Fields, Field Field, ISensorReadout Request);

		/// <summary>
		/// Gets an array of control parameters
		/// </summary>
		ControlParameter[] GetControlParameters();

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		IMqttData CreateNew(MqttTopic Topic, MqttContent Content);
	}
}
