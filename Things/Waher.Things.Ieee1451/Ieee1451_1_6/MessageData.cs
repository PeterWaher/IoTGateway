using System;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Data mode of topic
	/// </summary>
	public enum DataMode
	{
		/// <summary>
		/// Binary
		/// </summary>
		Binary,

		/// <summary>
		/// Base64
		/// </summary>
		Base64,

		/// <summary>
		/// Hexadecimal
		/// </summary>
		Hex
	}

	/// <summary>
	/// Encapsulates messages from an IEEE1451.1.6 device.
	/// </summary>
	public class MessageData : MqttData
	{
		private readonly byte[] ncapId;
		private readonly byte[] timId;
		private readonly ushort channelId;
		private readonly string communicationTopic;
		private DataMode? dataMode = null;

		/// <summary>
		/// Encapsulates messages from an IEEE1451.1.6 device.
		/// </summary>
		public MessageData()
			: base()
		{
		}

		/// <summary>
		/// Encapsulates messages from an IEEE1451.1.6 device.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID</param>
		/// <param name="ChannelId">Channel ID</param>
		public MessageData(MqttTopic Topic, byte[] NcapId, byte[] TimId, ushort ChannelId)
			: base(Topic)
		{
			this.ncapId = NcapId;
			this.timId = TimId;
			this.channelId = ChannelId;

			this.communicationTopic = this.EvaluateCommunicationTopic();
		}

		/// <summary>
		/// Encapsulates messages from an IEEE1451.1.6 device.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Message">Parsed message.</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID</param>
		/// <param name="ChannelId">Channel ID</param>
		public MessageData(MqttTopic Topic, Message Message, byte[] NcapId, byte[] TimId, ushort ChannelId)
			: this(Topic, NcapId, TimId, ChannelId)
		{
			MessageSwitch.DataReported(Message, NcapId, TimId, ChannelId);
		}

		/// <summary>
		/// NCAP ID
		/// </summary>
		public byte[] NcapId => this.ncapId;

		/// <summary>
		/// TIM ID (can be null, if addressing the NCAP)
		/// </summary>
		public byte[] TimId => this.timId;

		/// <summary>
		/// Channel ID (can be 0, if addressing the TIM, or NCAP)
		/// </summary>
		public int ChannelId => this.channelId;

		/// <summary>
		/// Called when new data has been received.
		/// </summary>
		/// <param name="Message">New parsed message.</param>
		/// <returns>If response to a pending request was received (true)</returns>
		public bool DataReported(Message Message)
		{
			return MessageSwitch.DataReported(Message, this.ncapId, this.timId, this.channelId);
		}

		private string EvaluateCommunicationTopic()
		{
			string s = this.Topic?.FullTopic;
			if (string.IsNullOrEmpty(s))
				return s;

			int i;

			if (!MessageSwitch.IsZero(this.timId))
			{
				if (this.channelId > 0)
				{
					i = s.LastIndexOf('/');
					if (i < 0)
						return s;

					s = s.Substring(0, i);
				}

				i = s.LastIndexOf('/');
				if (i < 0)
					return s;

				s = s.Substring(0, i);
			}

			i = s.LastIndexOf('/');
			if (i < 0)
				return s;

			return s.Substring(0, i);
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
		/// <returns>Data processing result</returns>
		public override Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content)
		{
			byte[] Data;

			if (!this.dataMode.HasValue)
			{
				string s = Content.DataString;

				if (HexStringData.RegEx.IsMatch(s))
					this.dataMode = DataMode.Hex;
				else if (Base64Data.RegEx.IsMatch(s))
					this.dataMode = DataMode.Base64;
				else
					this.dataMode = DataMode.Binary;
			}

			switch (this.dataMode.Value)
			{
				case DataMode.Binary:
				default:
					Data = Content.Data;
					break;

				case DataMode.Base64:
					try
					{
						Data = Convert.FromBase64String(Content.DataString);
					}
					catch (Exception)
					{
						return Task.FromResult(DataProcessingResult.Incompatible);
					}
					break;

				case DataMode.Hex:
					try
					{
						Data = Hashes.StringToBinary(Content.DataString);
					}
					catch (Exception)
					{
						return Task.FromResult(DataProcessingResult.Incompatible);
					}
					break;
			}

			if (!Ieee1451Parser.TryParseMessage(Data, out Message Message))
				return Task.FromResult(DataProcessingResult.Incompatible);

			return Ncap.MessageReceived(this, Topic, Message);
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(RootTopic), 15, "IEEE 1451.1.6 message data");
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, "Transducer data");
		}

		/// <summary>
		/// Requests transducer data from an NCAP.
		/// </summary>
		/// <param name="SamplingMode">Sample mode.</param>
		/// <param name="TimeoutMilliseconds">Maximum amount of time to wait for
		/// a response.</param>
		/// <param name="StaleLimitSeconds">A received message is considered stale, if
		/// older than this number of seconds.</param>
		/// <returns>Response</returns>
		/// <exception cref="TimeoutException">If no response has been received within
		/// the prescribed time.</exception>
		public async Task<TransducerAccessMessage> RequestTransducerData(SamplingMode SamplingMode,
			int TimeoutMilliseconds, int StaleLimitSeconds)
		{
			byte[] Request = TransducerAccessMessage.SerializeRequest(this.ncapId,
				this.timId, this.channelId, SamplingMode, TimeoutMilliseconds * 1e-3);

			Task<TransducerAccessMessage> Result = MessageSwitch.WaitForMessage<TransducerAccessMessage>(
				TimeoutMilliseconds, StaleLimitSeconds, this.ncapId, this.timId, this.channelId);

			if (!Result.IsCompleted)
				await this.Topic.Broker.Publish(this.communicationTopic, MqttQualityOfService.AtLeastOnce, false, Request);

			return await Result;

			// TODO: Check correct NCAP, TIM & Channel IDs
		}

		/// <summary>
		/// Requests transducer data from an NCAP.
		/// </summary>
		/// <param name="SamplingMode">Sample mode.</param>
		/// <param name="TimeoutMilliseconds">Maximum amount of time to wait for
		/// a response.</param>
		/// <param name="StaleLimitSeconds">A received message is considered stale, if
		/// older than this number of seconds.</param>
		/// <returns>Response</returns>
		/// <exception cref="TimeoutException">If no response has been received within
		/// the prescribed time.</exception>
		public async Task<TedsAccessMessage> RequestTEDS(SamplingMode SamplingMode,
			int TimeoutMilliseconds, int StaleLimitSeconds)
		{
			byte[] Request = TransducerAccessMessage.SerializeRequest(this.ncapId,
				this.timId, this.channelId, SamplingMode, TimeoutMilliseconds * 1e-3);

			Task<TedsAccessMessage> Result = MessageSwitch.WaitForMessage<TedsAccessMessage>(
				TimeoutMilliseconds, StaleLimitSeconds, this.ncapId, this.timId, this.channelId);

			if (!Result.IsCompleted)
				await this.Topic.Broker.Publish(this.communicationTopic, MqttQualityOfService.AtLeastOnce, false, Request);

			return await Result;

			// TODO: Check correct NCAP, TIM & Channel IDs
		}

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override async Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			// TODO: Configurable timeout.
			// TODO: Configurable stale limit.

			try
			{
				if (this.channelId > 0)         // Channel
				{
					TransducerAccessMessage Data = await this.RequestTransducerData(SamplingMode.Immediate, 10000, 60);

					if (Data.TryParseTransducerData(ThingReference,
						out ushort ErrorCode, out TransducerData ParsedData))
					{
						if (ErrorCode != 0)
							Request.ReportErrors(false, new ThingError(ThingReference, "Transducer Error code: " + ErrorCode.ToString("X4")));

						Request.ReportFields(true, ParsedData.Fields);
					}
					else
						Request.ReportErrors(true, new ThingError(ThingReference, "Unable to parse transducer data."));
				}
				else if (!MessageSwitch.IsZero(this.timId))   // TIM
				{
				}
				else                            // NCAP
				{
				}
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(ThingReference, ex.Message));
			}

			/*
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

			*/
		}
	}
}
