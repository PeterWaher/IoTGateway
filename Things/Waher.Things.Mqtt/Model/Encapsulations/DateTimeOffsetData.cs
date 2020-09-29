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
	/// Represents an MQTT topic with Date &amp; Time &amp; Offset data.
	/// </summary>
	public class DateTimeOffsetData : Data
	{
		private DateTimeOffset value;

		/// <summary>
		/// Represents an MQTT topic with Date &amp; Time &amp; Offset data.
		/// </summary>
		public DateTimeOffsetData(MqttTopic Topic, DateTimeOffset Value)
			: base(Topic)
		{
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			if (CommonTypes.TryParseRfc822(CommonTypes.GetString(Content.Data, Encoding.UTF8), out DateTimeOffset Value))
			{
				this.value = Value;
				this.timestamp = DateTime.Now;
				this.qos = Content.Header.QualityOfService;
				this.retain = Content.Header.Retain;
			}
			else
				throw new Exception("Invalid web date & time value.");
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 33, "Date and Time");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new DateTimeField(ThingReference, this.timestamp, this.Append(Prefix, "Value"),
				this.value.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new DateTimeControlParameter("Timestamp", "Publish", "Timestamp:", "Date & time portion of topic.", null, null,
					(n) => Task.FromResult<DateTime?>(this.value.DateTime),
					(n, v) =>
					{
						this.value = new DateTimeOffset(v, this.value.Offset);
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(CommonTypes.EncodeRfc822(this.value)));
						return Task.CompletedTask;
					}),
				new TimeControlParameter("Offset", "Publish", "Time zone:", "Time zone portion of topic.", null, null,
					(n) => Task.FromResult<TimeSpan?>(this.value.Offset),
					(n, v) =>
					{
						this.value = new DateTimeOffset(this.value.DateTime, v);
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(CommonTypes.EncodeRfc822(this.value)));
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
