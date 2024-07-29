using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with boolean data.
	/// </summary>
	public class BooleanData : Data
	{
		private bool value;

		/// <summary>
		/// Represents an MQTT topic with boolean data.
		/// </summary>
		public BooleanData(MqttTopic Topic, bool Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void DataReported(MqttContent Content)
		{
			string s = CommonTypes.GetString(Content.Data, Encoding.UTF8);
			if (CommonTypes.TryParse(s, out bool b))
			{
				this.value = b;
				this.timestamp = DateTime.Now;
				this.qos = Content.Header.QualityOfService;
				this.retain = Content.Header.Retain;
			}
			else
				throw new Exception("Invalid boolean value.");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 32, "Boolean");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new BooleanField(ThingReference, this.timestamp, this.Append(Prefix, "Value"),
				this.value, FieldType.Momentary, FieldQoS.AutomaticReadout));
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
				new BooleanControlParameter("Value", "Publish", "Value.", "Boolean value of topic.",
					(n) => Task.FromResult<bool?>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(CommonTypes.Encode(v)));
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
	}
}
