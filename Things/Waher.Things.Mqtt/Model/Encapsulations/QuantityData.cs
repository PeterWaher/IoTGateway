using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with Physical Quantity data.
	/// </summary>
	public class QuantityData : MqttData
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Regex RegEx = new Regex(@"^(?'Magnitude'[+-]?\d*([.]\d*)?([eE][+-]?\d+)?)\s+(?'Unit'[^\s]+)$", RegexOptions.Compiled | RegexOptions.Singleline);

		private string unit;
		private double value;
		private byte nrDecimals;

		/// <summary>
		/// Represents an MQTT topic with Physical Quantity data.
		/// </summary>
		public QuantityData()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with Physical Quantity data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <param name="Unit">Value unit.</param>
		public QuantityData(MqttTopic Topic, double Value, byte NrDecimals, string Unit)
			: base(Topic)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		public override bool DataReported(MqttTopic Topic, MqttContent Content)
		{
			string s = Content.DataString;
			Match M = RegEx.Match(s);
			if (M.Success && CommonTypes.TryParse(M.Groups["Magnitude"].Value, out this.value, out this.nrDecimals))
			{
				this.unit = M.Groups["Unit"].Value;
				this.Timestamp = DateTime.UtcNow;
				this.QoS = Content.Header.QualityOfService;
				this.Retain = Content.Header.Retain;

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 39, "Quantity");
		}

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new QuantityField(ThingReference, this.Timestamp, this.Append(Prefix, "Value"),
				this.value, this.nrDecimals, this.unit, FieldType.Momentary, FieldQoS.AutomaticReadout));
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
				new DoubleControlParameter("Value", "Publish", "Value:", "Units: " + this.unit,
					(n) => Task.FromResult<double?>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(CommonTypes.Encode(v, this.nrDecimals) + " " + this.unit));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString("F" + this.nrDecimals.ToString()) + " " + this.unit);
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Perfect;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new QuantityData(Topic, default, default, default);
			Result.DataReported(Topic, Content);
			return Result;
		}
	}
}
