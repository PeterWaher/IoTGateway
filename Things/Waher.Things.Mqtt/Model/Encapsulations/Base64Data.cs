using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with base64-encoded binary data.
	/// </summary>
	public class Base64Data : MqttData
	{
		/// <summary>
		/// TODO
		/// </summary>
		public const string RegExString = @"^\s*(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?\s*$";

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Regex RegEx = new Regex(RegExString, RegexOptions.Compiled | RegexOptions.Singleline);

		private byte[] value;

		/// <summary>
		/// Represents an MQTT topic with base64-encoded binary data.
		/// </summary>
		public Base64Data()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with base64-encoded binary data.
		/// </summary>
		public Base64Data(MqttTopic Topic, byte[] Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		public override bool DataReported(MqttContent Content)
		{
			string s = Content.DataString;

			if (RegEx.IsMatch(s))
			{
				try
				{
					this.value = Convert.FromBase64String(s);
					this.Timestamp = DateTime.UtcNow;
					this.QoS = Content.Header.QualityOfService;
					this.Retain = Content.Header.Retain;
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				return false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 42, "BASE-64");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new Int32Field(ThingReference, this.Timestamp, this.Append(Prefix, "#Bytes"), 
				this.value.Length, FieldType.Momentary, FieldQoS.AutomaticReadout));
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
				new StringControlParameter("Value", "Publish", "Value:", "BASE-64 value of topic.", RegExString,
					(n) => Task.FromResult<string>(Convert.ToBase64String(this.value)),
					(n, v) =>
					{
						this.value = Convert.FromBase64String(v);
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(v));
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

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Excellent;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic node</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new Base64Data(Topic, default);
			Result.DataReported(Content);
			return Result;
		}
	}
}
