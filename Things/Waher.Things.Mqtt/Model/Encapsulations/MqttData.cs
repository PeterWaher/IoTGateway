using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Abstract base class for MQTT data encapsulations.
	/// </summary>
	public abstract class MqttData : IMqttData
	{
		/// <summary>
		/// Abstract base class for MQTT data encapsulations.
		/// </summary>
		public MqttData()
		{
			this.Topic = new MqttTopic(null, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Abstract base class for MQTT data encapsulations.
		/// </summary>
		/// <param name="Topic">Topic node.</param>
		public MqttData(MqttTopic Topic)
		{
			this.Topic = Topic;
		}

		/// <summary>
		/// Timestamp of data reception.
		/// </summary>
		public DateTime Timestamp { get; protected set; } = DateTime.UtcNow;

		/// <summary>
		/// Quality of Service used
		/// </summary>
		public MqttQualityOfService QoS { get; protected set; } = MqttQualityOfService.AtLeastOnce;

		/// <summary>
		/// Topic used
		/// </summary>
		public MqttTopic Topic { get; }

		/// <summary>
		/// If retain flag was set.
		/// </summary>
		public bool Retain { get; protected set; }

		/// <summary>
		/// If data can be controlled (written)
		/// </summary>
		public virtual bool IsControllable => false;

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		public abstract bool DataReported(MqttTopic Topic, MqttContent Content);

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public abstract Task<string> GetTypeName(Language Language);

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public abstract void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last);

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public abstract void SnifferOutput(ISniffable Output);

		/// <summary>
		/// Outputs information to sniffer.
		/// </summary>
		public void Information(ISniffable Output, string Info)
		{
			foreach (ISniffer Sniffer in Output.Sniffers)
				Sniffer.Information(this.Topic.FullTopic + ": " + Info);
		}

		/// <summary>
		/// Appends a name to a topic name.
		/// </summary>
		protected string Append(string Prefix, string Name)
		{
			if (string.IsNullOrEmpty(Prefix))
				return Name;

			if (Name == "Value")
				return Prefix;
			else
				return Prefix + ", " + Name;
		}

		/// <summary>
		/// Reports fields during an active readout.
		/// </summary>
		public void Add(List<Field> Fields, Field Field, ISensorReadout Request)
		{
			if (Fields.Count > 50)
			{
				Request.ReportFields(false, Fields.ToArray());
				Fields.Clear();
			}

			Fields.Add(Field);
		}

		/// <summary>
		/// Gets an array of control parameters
		/// </summary>
		public virtual ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[0];
		}

		/// <summary>
		/// How well content of a specific type is supported.
		/// </summary>
		/// <param name="Content">Content object.</param>
		/// <returns>How well content of this type is supported.</returns>
		public Grade Supports(MqttContent Content)
		{
			return this.DataReported(null, Content) ? this.DefaultSupport : Grade.NotAtAll;
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public abstract Grade DefaultSupport { get; }

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public abstract IMqttData CreateNew(MqttTopic Topic, MqttContent Content);
	}
}
