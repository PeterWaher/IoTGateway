using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with Date &amp; Time &amp; Offset data.
	/// </summary>
	public class DateTimeOffsetData : MqttData
	{
		private DateTimeOffset value;

		/// <summary>
		/// Represents an MQTT topic with Date &amp; Time &amp; Offset data.
		/// </summary>
		public DateTimeOffsetData()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with Date &amp; Time &amp; Offset data.
		/// </summary>
		public DateTimeOffsetData(MqttTopic Topic, DateTimeOffset Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		public override bool DataReported(MqttContent Content)
		{
			if (DateTimeOffset.TryParse(Content.DataString, out DateTimeOffset Value) ||
				CommonTypes.TryParseRfc822(Content.DataString, out Value) ||
				XML.TryParse(Content.DataString, out Value))
			{
				this.value = Value;
				this.Timestamp = DateTime.UtcNow;
				this.QoS = Content.Header.QualityOfService;
				this.Retain = Content.Header.Retain;

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 33, "Date and Time");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new DateTimeField(ThingReference, this.Timestamp, this.Append(Prefix, "Value"),
				this.value.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout));
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
				new DateTimeControlParameter("Timestamp", "Publish", "Timestamp:", "Date & time portion of topic.", null, null,
					(n) => Task.FromResult<DateTime?>(this.value.DateTime),
					(n, v) =>
					{
						this.value = new DateTimeOffset(v, this.value.Offset);
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(CommonTypes.EncodeRfc822(this.value)));
						return Task.CompletedTask;
					}),
				new TimeControlParameter("Offset", "Publish", "Time zone:", "Time zone portion of topic.", null, null,
					(n) => Task.FromResult<TimeSpan?>(this.value.Offset),
					(n, v) =>
					{
						this.value = new DateTimeOffset(this.value.DateTime, v);
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(CommonTypes.EncodeRfc822(this.value)));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString());
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Perfect;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic node</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new DateTimeOffsetData(Topic, default);
			Result.DataReported(Content);
			return Result;
		}
	}
}
