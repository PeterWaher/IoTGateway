using System;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
    /// <summary>
    /// Base64-encoded IEEE 1451.1.6 NCAP.
    /// </summary>
    public class Base64Ncap : Ncap
	{
		private bool firstMessage = true;

		/// <summary>
		/// Base64-encoded IEEE 1451.1.6 NCAP.
		/// </summary>
		public Base64Ncap()
			: base()
		{
		}

		/// <summary>
		/// Base64-encoded IEEE 1451.1.6 NCAP.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		public Base64Ncap(MqttTopic Topic, byte[] Value)
			: base(Topic, Value)
		{
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node. If null, synchronous result should be returned.</param>
		/// <param name="Content">Published MQTT Content</param>
		/// <returns>Data processing result</returns>
		public override async Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content)
		{
			string s = Content.DataString;

			if (!Base64Data.RegEx.IsMatch(s))
				return this.firstMessage ? DataProcessingResult.Incompatible : DataProcessingResult.Processed;

			try
			{
				this.firstMessage = false;
				return await this.DataReported(Topic, Content, Convert.FromBase64String(s));
			}
			catch (Exception)
			{
				return DataProcessingResult.Processed;
			}
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(RootTopic), 12, "IEEE 1451.1.6 NCAP (BASE64)");
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ICommunicationLayer Output)
		{
		}

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new Base64Ncap(Topic, default);
			Result.DataReported(Topic, Content);
			return Result;
		}
	}
}
