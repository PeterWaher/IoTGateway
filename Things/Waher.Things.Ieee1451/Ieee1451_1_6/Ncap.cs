using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Abstract base class for IEEE 1451.1.6 NCAPs.
	/// </summary>
	public abstract class Ncap : MqttData
	{
		private byte[] value;

		/// <summary>
		/// Abstract base class for IEEE 1451.1.6 NCAPs.
		/// </summary>
		public Ncap()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for IEEE 1451.1.6 NCAPs.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		public Ncap(MqttTopic Topic, byte[] Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		/// <param name="Data">Binary data</param>
		public bool DataReported(MqttTopic Topic, MqttContent Content, byte[] Data)
		{
			if (!Ieee1451Parser.TryParseMessage(Data, out Message Message))
				return false;

			this.value = Data;
			this.Timestamp = DateTime.UtcNow;
			this.QoS = Content.Header.QualityOfService;
			this.Retain = Content.Header.Retain;

			this.MessageReceived(Topic, Message);

			return true;
		}

		/// <summary>
		/// Processes an IEEE 1451.0 message.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Message">Parsed binary message.</param>
		public virtual void MessageReceived(MqttTopic Topic, Message Message)
		{
		}

		private Task LogErrorAsync(string EventId, string Message)
		{
			return this.Topic?.Node?.LogErrorAsync(EventId, Message) ?? Task.CompletedTask;
		}

		private Task RemoveErrorAsync(string EventId)
		{
			return this.Topic?.Node?.RemoveErrorAsync(EventId) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Perfect;

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Data = new List<Field>()
			{
				new Int32Field(ThingReference, this.Timestamp, this.Append(Prefix, "#Bytes"),
					this.value?.Length ?? 0, FieldType.Momentary, FieldQoS.AutomaticReadout)
			};

			if (!(this.value is null) && this.value.Length <= 256)
			{
				Data.Add(new StringField(ThingReference, this.Timestamp, "Raw",
					Convert.ToBase64String(this.value), FieldType.Momentary, FieldQoS.AutomaticReadout));
			}

			Request.ReportFields(Last, Data);
		}
	}
}
