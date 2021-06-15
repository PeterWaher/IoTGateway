using System;
using System.Text;
using System.Text.RegularExpressions;
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
	/// Represents an MQTT topic with binary data encoded as decimal strings.
	/// </summary>
	public class HexStringData : Data
	{
		public const string RegExString = @"^\s*([A-Fa-f0-9]{2}){1,}\s*$";
		public static readonly Regex RegEx = new Regex(RegExString, RegexOptions.Compiled | RegexOptions.Singleline);

		private byte[] value;

		/// <summary>
		/// Represents an MQTT topic with binary data encoded as decimal strings.
		/// </summary>
		public HexStringData(MqttTopic Topic, byte[] Value)
			: base(Topic)
		{
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			this.value = Security.Hashes.StringToBinary(CommonTypes.GetString(Content.Data, Encoding.ASCII));
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 43, "HEX");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new Int32Field(ThingReference, this.timestamp, this.Append(Prefix, "#Bytes"), 
				this.value.Length, FieldType.Momentary, FieldQoS.AutomaticReadout));
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new StringControlParameter("Value", "Publish", "Value:", "HEX value of topic.", RegExString,
					(n) => Task.FromResult<string>(Security.Hashes.BinaryToString(this.value)),
					(n, v) =>
					{
						this.value = Security.Hashes.StringToBinary(v);
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v));
						return Task.CompletedTask;
					})
			};
		}

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
