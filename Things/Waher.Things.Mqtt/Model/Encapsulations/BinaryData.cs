using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with binary data.
	/// </summary>
	public class BinaryData : Data
	{
		private byte[] value;

		/// <summary>
		/// Represents an MQTT topic with binary data.
		/// </summary>
		public BinaryData(MqttTopic Topic, byte[] Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void DataReported(MqttContent Content)
		{
			this.value = Content.Data;
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 31, "Binary");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Data = new List<Field>()
			{
				new Int32Field(ThingReference, this.timestamp, this.Append(Prefix, "#Bytes"),
					this.value?.Length ?? 0, FieldType.Momentary, FieldQoS.AutomaticReadout)
			};

			if (!(this.value is null) && this.value.Length <= 256)
			{
				Data.Add(new StringField(ThingReference, this.timestamp, "Raw",
					Convert.ToBase64String(this.value), FieldType.Momentary, FieldQoS.AutomaticReadout));
			}

			Request.ReportFields(Last, Data);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override bool IsControllable => true;

		/// <summary>
		/// TODO
		/// </summary>
		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new StringControlParameter("Value", "Publish", "Value:", "BASE-64 value of topic.", Base64Data.RegExString,
					(n) => Task.FromResult<string>(Convert.ToBase64String(this.value)),
					(n, v) =>
					{
						this.value = Convert.FromBase64String(v);
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, this.value);
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			if (this.value is null)
				this.Information(Output, "NULL");
			else if (this.value.Length == 1)
				this.Information(Output, "1 byte.");
			else
				this.Information(Output, this.value.Length.ToString() + " bytes.");
		}
	}
}
