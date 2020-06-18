using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with Date &amp; Time data.
	/// </summary>
	public class DateTimeData : Data
	{
		private DateTime value;

		/// <summary>
		/// Represents an MQTT topic with Date &amp; Time data.
		/// </summary>
		public DateTimeData(MqttTopic Topic, DateTime Value)
			: base(Topic)
		{
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			this.value = DateTime.Parse(Encoding.UTF8.GetString(Content.Data));
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 33, "Date and Time");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			if (this.value.TimeOfDay == TimeSpan.Zero)
			{
				Request.ReportFields(Last, new DateField(ThingReference, this.timestamp, this.Append(Prefix, "Value"),
					this.value, FieldType.Momentary, FieldQoS.AutomaticReadout));
			}
			else
			{
				Request.ReportFields(Last, new DateTimeField(ThingReference, this.timestamp, this.Append(Prefix, "Value"),
					this.value, FieldType.Momentary, FieldQoS.AutomaticReadout));
			}
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new DateTimeControlParameter("Value", "Publish", "Value:", "Date & time value of topic.", null, null,
					(n) => Task.FromResult<DateTime?>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v.ToString()));
						return Task.CompletedTask;
					})
			};
		}

		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString());
		}
	}
}
