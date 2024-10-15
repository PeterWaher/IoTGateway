using System;
using System.Text;
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
	/// Represents an MQTT topic with Floating-point data.
	/// </summary>
	public class FloatingPointData : MqttData
	{
		private double value;
		private byte nrDecimals;

		/// <summary>
		/// Represents an MQTT topic with Floating-point data.
		/// </summary>
		public FloatingPointData()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with Floating-point data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		public FloatingPointData(MqttTopic Topic, double Value, byte NrDecimals)
			: base(Topic)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node. If null, synchronous result should be returned.</param>
		/// <param name="Content">Published MQTT Content</param>
		/// <returns>Data processing result</returns>
		public override Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content)
		{
			if (CommonTypes.TryParse(Content.DataString, out double x, out byte NrDecimals))
			{
				this.value = x;
				this.nrDecimals = NrDecimals;
				this.Timestamp = DateTime.UtcNow;
				this.QoS = Content.Header.QualityOfService;
				this.Retain = Content.Header.Retain;

				return Task.FromResult(DataProcessingResult.ProcessedNewMomentaryValues);
			}
			else
				return Task.FromResult(DataProcessingResult.Incompatible);
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 35, "Floating-point");
		}

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			Request.ReportFields(Last, new QuantityField(ThingReference, this.Timestamp, this.Append(Prefix, "Value"), 
				this.value, this.nrDecimals, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout));
		
			return Task.CompletedTask;
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
				new DoubleControlParameter("Value", "Publish", "Value:", "Floating-point value of topic.",
					(n) => Task.FromResult<double?>(this.value),
					(n, v) =>
					{
						this.value = v;
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(CommonTypes.Encode(v, this.nrDecimals)));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.value.ToString());
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Excellent;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new FloatingPointData(Topic, default, default);
			Result.DataReported(Topic, Content);
			return Result;
		}
	}
}
