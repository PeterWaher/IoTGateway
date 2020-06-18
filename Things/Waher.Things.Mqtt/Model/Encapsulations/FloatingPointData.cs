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
	/// Represents an MQTT topic with Floating-point data.
	/// </summary>
	public class FloatingPointData : Data
	{
		private double value;
		private byte nrDecimals;

		/// <summary>
		/// Represents an MQTT topic with Floating-point data.
		/// </summary>
		public FloatingPointData(MqttTopic Topic, double Value, byte NrDecimals)
			: base(Topic)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
		}

		public override void DataReported(MqttContent Content)
		{
			string s = Encoding.UTF8.GetString(Content.Data);
			if (CommonTypes.TryParse(s, out double x, out this.nrDecimals))
			{
				this.value = x;
				this.timestamp = DateTime.Now;
				this.qos = Content.Header.QualityOfService;
				this.retain = Content.Header.Retain;
			}
			else
				throw new Exception("Invalid floating-point value.");
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 35, "Floating-point");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new QuantityField(ThingReference, this.timestamp, this.Append(Prefix, "Value"), 
				this.value, this.nrDecimals, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new DoubleControlParameter("Value", "Publish", "Value:", "Floating-point value of topic.",
					(n) => Task.FromResult<double?>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(CommonTypes.Encode(v, this.nrDecimals)));
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
