using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
	/// Represents an MQTT topic with Physical Quantity data.
	/// </summary>
	public class QuantityData : Data
	{
		public static readonly Regex RegEx = new Regex(@"^(?'Magnitude'[+-]?\d*([.]\d*)?([eE][+-]?\d+)?)\s+(?'Unit'[^\s]+)$", RegexOptions.Compiled | RegexOptions.Singleline);

		private string unit;
		private double value;
		private byte nrDecimals;

		/// <summary>
		/// Represents an MQTT topic with Physical Quantity data.
		/// </summary>
		public QuantityData(MqttTopic Topic, double Value, byte NrDecimals, string Unit)
			: base(Topic)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		public override void DataReported(MqttContent Content)
		{
			string s = Encoding.UTF8.GetString(Content.Data);
			Match M = RegEx.Match(s);
			if (M.Success && CommonTypes.TryParse(M.Groups["Magnitude"].Value, out this.value, out this.nrDecimals))
			{
				this.unit = M.Groups["Unit"].Value;
				this.timestamp = DateTime.Now;
				this.qos = Content.Header.QualityOfService;
				this.retain = Content.Header.Retain;
			}
			else
				throw new Exception("Invalid physical quantity value.");
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 39, "Quantity");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new QuantityField(ThingReference, this.timestamp, this.Append(Prefix, "Value"),
				this.value, this.nrDecimals, this.unit, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new DoubleControlParameter("Value", "Publish", "Value:", "Units: " + this.unit,
					(n) => this.value,
					(n, v) =>
					{
						this.value = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(CommonTypes.Encode(v, this.nrDecimals) + " " + this.unit));
					})
			};
		}

		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString("F" + this.nrDecimals.ToString()) + " " + this.unit);
		}
	}
}
