using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Binary-encoded IEEE 1451.1.6 NCAP.
	/// </summary>
	public class BinaryNcap : Ncap
	{
		/// <summary>
		/// Binary-encoded IEEE 1451.1.6 NCAP.
		/// </summary>
		public BinaryNcap()
			: base()
		{
		}

		/// <summary>
		/// Binary-encoded IEEE 1451.1.6 NCAP.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		public BinaryNcap(MqttTopic Topic, byte[] Value)
			: base(Topic, Value)
		{
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node. If null, synchronous result should be returned.</param>
		/// <param name="Content">Published MQTT Content</param>
		public override Task<bool> DataReported(MqttTopic Topic, MqttContent Content)
		{
			return this.DataReported(Topic, Content, Content.Data);
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 4, "IEEE 1451.1.6 NCAP (BINARY)");
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, "BINARY-encoded binary IEEE 1451.1.6 message");
		}

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new BinaryNcap(Topic, default);
			Result.DataReported(Topic, Content);
			return Result;
		}
	}
}
