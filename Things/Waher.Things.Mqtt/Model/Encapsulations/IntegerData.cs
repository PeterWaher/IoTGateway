using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with integer data.
	/// </summary>
	public class IntegerData : Data
	{
		private long value;

		/// <summary>
		/// Represents an MQTT topic with integer data.
		/// </summary>
		public IntegerData(MqttTopic Topic, long Value)
			: base(Topic)
		{
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			this.value = long.Parse(Encoding.UTF8.GetString(Content.Data));
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 37, "Integer");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new Int64Field(ThingReference, this.timestamp, this.Append(Prefix, "Value"), 
				this.value, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new Int64ControlParameter("Value", "Publish", "Value:", "Integer value of topic.",
					(n) => this.value,
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v.ToString()));
					})
			};
		}

		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString());
		}
	}
}
