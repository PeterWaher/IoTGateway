using System;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Encapsulates transducer channel data.
	/// </summary>
	public class ChannelData : MqttData
	{
		private TransducerAccessMessage data;
		private TedsAccessMessage teds;
		private DateTime tedsTimestamp;

		/// <summary>
		/// Encapsulates transducer channel data.
		/// </summary>
		public ChannelData()
			: base()
		{
		}

		/// <summary>
		/// Encapsulates transducer channel data.
		/// </summary>
		/// <param name="Message">Parsed message.</param>
		public ChannelData(TransducerAccessMessage Message)
		{
			this.data = Message;
		}

		/// <summary>
		/// Encapsulates transducer channel data.
		/// </summary>
		/// <param name="Message">Parsed message.</param>
		public ChannelData(TedsAccessMessage Message)
		{
			this.teds = Message;
			this.tedsTimestamp = DateTime.UtcNow;
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.NotAtAll;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			return null;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node. If null, synchronous result should be returned.</param>
		/// <param name="Content">Published MQTT Content</param>
		public override Task<bool> DataReported(MqttTopic Topic, MqttContent Content)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Message">New parsed message.</param>
		public void DataReported(TransducerAccessMessage Message)
		{
			this.data = Message;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Message">New parsed message.</param>
		public void DataReported(TedsAccessMessage Message)
		{
			this.teds = Message;
			this.tedsTimestamp = DateTime.UtcNow;
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 8, "Transducer data.");
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, "Transducer data");
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
			if (this.teds is null)
				Request.ReportErrors(false, new ThingError(ThingReference, "No TEDS received."));
			else if (this.teds.TryParseTeds(true, out ushort ErrorCode, out Teds ParsedTeds))
			{
				if (ErrorCode != 0)
					Request.ReportErrors(false, new ThingError(ThingReference, "TEDS Error code: " + ErrorCode.ToString("X4")));

				Request.ReportFields(false, ParsedTeds.GetFields(ThingReference, this.tedsTimestamp));
			}
			else
				Request.ReportErrors(false, new ThingError(ThingReference, "Unable to parse TEDS received."));

			if (this.data is null)
				Request.ReportErrors(true, new ThingError(ThingReference, "No data received."));
			else if (this.data.TryParseTransducerData(ThingReference,
				out ushort ErrorCode, out TransducerData ParsedData))
			{
				if (ErrorCode != 0)
					Request.ReportErrors(false, new ThingError(ThingReference, "Transducer Error code: " + ErrorCode.ToString("X4")));

				Request.ReportFields(true, ParsedData.Fields);
			}
			else
				Request.ReportErrors(true, new ThingError(ThingReference, "Unable to parse transducer data received."));

			return Task.CompletedTask;
		}
	}
}
