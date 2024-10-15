using System;
using System.Collections.Generic;
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
	/// Encapsulates messages from an IEEE1451.1.6 device.
	/// </summary>
	public class MessageData : MqttData
	{
		private readonly Dictionary<Type, MessageRec> messages = new Dictionary<Type, MessageRec>();
		private readonly byte[] ncapId;
		private readonly byte[] timId;
		private readonly ushort channelId;
		private readonly string communicationTopic;

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
			lock (this.messages)
			{
				this.messages[Message.GetType()] = new MessageRec()
				{
					Message = Message,
					Timestamp = DateTime.UtcNow,
					Pending = null
				};
			}
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

		private class MessageRec
		{
			public Message Message;
			public DateTime Timestamp;
			public TaskCompletionSource<Message> Pending;
		}

		private string EvaluateCommunicationTopic()
		{
			string s = this.Topic?.FullTopic;
			if (string.IsNullOrEmpty(s))
				return s;

			int i;

			if (!IsZero(this.timId))
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
		/// Checks if an ID is "zero", i.e. contains only zero bytes.
		/// </summary>
		/// <param name="A"></param>
		/// <returns></returns>
		public static bool IsZero(byte[] A)
		{
			if (A is null)
				return true;

			foreach (byte b in A)
			{
				if (b != 0)
					return false;
			}

			return true;
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
			return Task.FromResult(DataProcessingResult.Incompatible);
		}

		/// <summary>
		/// Called when new data has been received.
		/// </summary>
		/// <param name="Message">New parsed message.</param>
		/// <returns>If response to a pending request was received (true)</returns>
		public bool DataReported(Message Message)
		{
			Type T = Message.GetType();

			lock (this.messages)
			{
				if (this.messages.TryGetValue(T, out MessageRec Rec))
				{
					Rec.Message = Message;
					Rec.Timestamp = DateTime.UtcNow;

					if (!(Rec.Pending is null))
					{
						Rec.Pending.TrySetResult(Message);
						Rec.Pending = null;
						return true;
					}
				}
				else
				{
					this.messages[T] = new MessageRec()
					{
						Message = Message,
						Timestamp = DateTime.UtcNow,
						Pending = null
					};
				}
			}

			return false;
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

			Task<TransducerAccessMessage> Result = this.WaitForMessage<TransducerAccessMessage>(TimeoutMilliseconds, StaleLimitSeconds);

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

			Task<TedsAccessMessage> Result = this.WaitForMessage<TedsAccessMessage>(TimeoutMilliseconds, StaleLimitSeconds);

			if (!Result.IsCompleted)
				await this.Topic.Broker.Publish(this.communicationTopic, MqttQualityOfService.AtLeastOnce, false, Request);

			return await Result;

			// TODO: Check correct NCAP, TIM & Channel IDs
		}

		/// <summary>
		/// Waits for a message to be received.
		/// </summary>
		/// <typeparam name="T">Type of message expected.</typeparam>
		/// <param name="StaleLimitSeconds">A received message is considered stale, if
		/// older than this number of seconds.</param>
		/// <param name="TimeoutMilliseconds">Maximum amount of time to wait for the message.</param>
		/// <returns>Message</returns>
		/// <exception cref="TimeoutException">If no message is received within the
		/// prescribed time.</exception>
		public async Task<T> WaitForMessage<T>(int TimeoutMilliseconds, int StaleLimitSeconds)
			where T : Message
		{
			TaskCompletionSource<Message> Pending = new TaskCompletionSource<Message>();
			TaskCompletionSource<Message> Obsolete = null;
			Type MessageType = typeof(T);

			lock (this.messages)
			{
				if (this.messages.TryGetValue(MessageType, out MessageRec Rec))
				{
					if (!(Rec.Message is null) && DateTime.UtcNow.Subtract(Rec.Timestamp).TotalSeconds < StaleLimitSeconds)
						return (T)Rec.Message;

					Obsolete = Rec.Pending;
					Rec.Pending = Pending;
				}
				else
				{
					this.messages[MessageType] = new MessageRec()
					{
						Message = null,
						Timestamp = DateTime.MinValue,
						Pending = Pending
					};
				}
			}

			_ = Task.Delay(TimeoutMilliseconds).ContinueWith(_ =>
			{
				Pending.TrySetResult(null);
			});

			Message Result = await Pending.Task;

			if (Result is null)
			{
				lock (this.messages)
				{
					if (this.messages.TryGetValue(MessageType, out MessageRec Rec) &&
						Rec.Pending == Pending)
					{
						Rec.Pending = null;
					}
				}

				throw new TimeoutException();
			}

			return (T)Result;
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
					TransducerAccessMessage Data = await this.RequestTransducerData(SamplingMode.Immediate, 5000, 60);

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
				else if (!IsZero(this.timId))   // TIM
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
