using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Units;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

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
		private readonly Dictionary<TedsAccessCode, KeyValuePair<DateTime, Teds>> teds = new Dictionary<TedsAccessCode, KeyValuePair<DateTime, Teds>>();
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
		public override async Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content)
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
						return DataProcessingResult.Processed;
					}
					break;

				case DataMode.Hex:
					try
					{
						Data = Hashes.StringToBinary(Content.DataString);
					}
					catch (Exception)
					{
						return DataProcessingResult.Processed;
					}
					break;
			}

			Message Message = await Ieee1451Parser.TryParseMessage(Data, Content.Sniffable);
			if (Message is null)
				return DataProcessingResult.Processed;

			return await Ncap.MessageReceived(this, Topic, Message);
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
		}

		/// <summary>
		/// Requests transducer data from an NCAP.
		/// </summary>
		/// <param name="TedsAccessCode">What TEDS to request.</param>
		/// <param name="TimeoutMilliseconds">Maximum amount of time to wait for
		/// a response.</param>
		/// <param name="StaleLimitSeconds">A received message is considered stale, if
		/// older than this number of seconds.</param>
		/// <returns>Response</returns>
		/// <exception cref="TimeoutException">If no response has been received within
		/// the prescribed time.</exception>
		public async Task<TedsAccessMessage> RequestTEDS(TedsAccessCode TedsAccessCode,
			int TimeoutMilliseconds, int StaleLimitSeconds)
		{
			byte[] Request = TedsAccessMessage.SerializeRequest(this.ncapId,
				this.timId, this.channelId, TedsAccessCode, 0, TimeoutMilliseconds * 1e-3);

			Task<TedsAccessMessage> Result = MessageSwitch.WaitForMessage<TedsAccessMessage>(
				TimeoutMilliseconds, StaleLimitSeconds, this.ncapId, this.timId, this.channelId);

			if (!Result.IsCompleted)
				await this.Topic.Broker.Publish(this.communicationTopic, MqttQualityOfService.AtLeastOnce, false, Request);

			return await Result;
		}

		private async Task<(Teds, DateTime)> GetTeds(TedsAccessCode Code, ThingReference ThingReference, ISensorReadout Request)
		{
			DateTime TP = DateTime.UtcNow;

			this.GetTimeouts(out int TimeoutMilliseconds, out int _, out int RefreshTedsHours, out _);

			lock (this.teds)
			{
				if (this.teds.TryGetValue(Code, out KeyValuePair<DateTime, Teds> P) && TP.Subtract(P.Key).TotalHours < RefreshTedsHours)
					return (P.Value, P.Key);
			}

			TedsAccessMessage TedsMessage = await this.RequestTEDS(Code, TimeoutMilliseconds, 0);   // Do not use cached message. Force readout.

			(ushort ErrorCode, Teds Teds) = await TedsMessage.TryParseTeds();
			if (!(Teds is null))
			{
				if (ErrorCode != 0)
					Request.ReportErrors(false, new ThingError(ThingReference, "Transducer Error code: " + ErrorCode.ToString("X4")));
				else
				{
					lock (this.teds)
					{
						this.teds[Code] = new KeyValuePair<DateTime, Teds>(TP, Teds);
					}

					return (Teds, TP);
				}
			}
			else
				Request.ReportErrors(true, new ThingError(ThingReference, "Unable to parse transducer data."));

			return (null, DateTime.MinValue);
		}

		private void GetTimeouts(out int TimeoutMilliseconds, out int StaleSeconds, out int RefreshTedsHours, out Unit PreferredUnit)
		{
			if (this.Topic.Node is MqttNcapTopicNode NcapNode)
			{
				TimeoutMilliseconds = NcapNode.TimeoutMilliseconds;
				StaleSeconds = NcapNode.StaleSeconds;
				RefreshTedsHours = NcapNode.RefreshTedsHours;

				if (!(NcapNode is MqttChannelTopicNode ChannelNode) || 
					string.IsNullOrEmpty(ChannelNode.PreferredUnit) ||
					!Unit.TryParse(ChannelNode.PreferredUnit, out PreferredUnit))
				{
					PreferredUnit = null;
				}
			}
			else
			{
				TimeoutMilliseconds = 10000;
				StaleSeconds = 60;
				RefreshTedsHours = 24;
				PreferredUnit = null;
			}
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
			try
			{
				this.GetTimeouts(out int TimeoutMilliseconds, out int StaleSeconds, out int RefreshTedsHours, out Unit PreferredUnit);

				if (this.channelId > 0)         // Channel
				{
					(Teds Teds, DateTime TedsTimestamp) = await this.GetTeds(TedsAccessCode.ChanTEDS, ThingReference, Request);

					if (Request.IsIncluded(FieldType.Identity) || Request.IsIncluded(FieldType.Status))
					{
						Field[] Fields = Teds.GetFields(ThingReference, TedsTimestamp);
						Request.ReportFields(false, Fields);

						(Teds MetaTeds, DateTime MetaTedsTimestamp) = await this.GetTeds(TedsAccessCode.MetaTEDS, ThingReference, Request);
						Field[] Fields2 = MetaTeds.GetFields(ThingReference, MetaTedsTimestamp);
						Field[] Fields3 = RemoveDuplicates(Fields2, Fields);

						if (!(Fields3 is null))
							Request.ReportFields(false, Fields3);
					}

					TransducerAccessMessage TransducerMessage = await this.RequestTransducerData(SamplingMode.Immediate, TimeoutMilliseconds, StaleSeconds);

					if (TransducerMessage.TryParseTransducerData(ThingReference, Teds, PreferredUnit, out ushort ErrorCode, out TransducerData TransducerData))
					{
						if (ErrorCode != 0)
							Request.ReportErrors(false, new ThingError(ThingReference, "Transducer Error code: " + ErrorCode.ToString("X4")));

						Request.ReportFields(true, TransducerData.Fields);
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
		}

		private static Field[] RemoveDuplicates(Field[] Fields, Field[] AlreadyReported)
		{
			Dictionary<string, Field> ByName = new Dictionary<string, Field>();

			foreach (Field F in AlreadyReported)
				ByName[F.Name] = F;

			List<Field> Result = null;

			foreach (Field F in Fields)
			{
				if (ByName.TryGetValue(F.Name, out Field F2) && F.ObjectValue.Equals(F2.ObjectValue))
					continue;

				if (Result is null)
					Result = new List<Field>();

				Result.Add(F);
			}

			return Result?.ToArray();
		}
	}
}
