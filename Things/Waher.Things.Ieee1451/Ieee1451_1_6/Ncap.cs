using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Abstract base class for IEEE 1451.1.6 NCAPs.
	/// </summary>
	public abstract class Ncap : MqttData
	{
		private byte[] value;

		/// <summary>
		/// Abstract base class for IEEE 1451.1.6 NCAPs.
		/// </summary>
		public Ncap()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for IEEE 1451.1.6 NCAPs.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Value">Data value</param>
		public Ncap(MqttTopic Topic, byte[] Value)
			: base(Topic)
		{
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		/// <param name="Data">Binary data</param>
		/// <returns>Data processing result</returns>
		public async Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content, byte[] Data)
		{
			if (!Ieee1451Parser.TryParseMessage(Data, out Message Message))
				return DataProcessingResult.Incompatible;

			this.value = Data;
			this.Timestamp = DateTime.UtcNow;
			this.QoS = Content.Header.QualityOfService;
			this.Retain = Content.Header.Retain;

			if (Topic is null)
				return DataProcessingResult.Processed;

			return await this.MessageReceived(Topic, Message);
		}

		/// <summary>
		/// Processes an IEEE 1451.0 message.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Message">Parsed binary message.</param>
		/// <returns>Data processing result</returns>
		public async Task<DataProcessingResult> MessageReceived(MqttTopic Topic, Message Message)
		{
			try
			{
				switch (Message.MessageType)
				{
					case MessageType.Reply:
						return await this.ProcessReply(Topic, Message);

					case MessageType.Command:
						await this.ProcessRequest(Message);
						break;

					case MessageType.Announcement:
					case MessageType.Notification:
					case MessageType.Callback:
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync(string.Empty, ex.Message);
			}

			return DataProcessingResult.Processed;
		}

		private async Task<DataProcessingResult> ProcessReply(MqttTopic Topic, Message Message)
		{
			MqttTopic SubTopic;
			bool ContainsMomentary = false;

			if (Message is TransducerAccessMessage TransducerAccessMessage)
			{
				ThingReference Ref = new ThingReference(this.Topic.Node);
				if (TransducerAccessMessage.TryParseTransducerData(Ref, out ushort ErrorCode, out TransducerData Data))
				{
					await this.RemoveErrorAsync("TransducerResponseError");

					StringBuilder sb = new StringBuilder();

					sb.Append(this.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Data.ChannelInfo.NcapId));

					if (!MessageData.IsZero(Data.ChannelInfo.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Data.ChannelInfo.TimId));

						if (Data.ChannelInfo.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Data.ChannelInfo.ChannelId.ToString());
						}
					}

					SubTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (ErrorCode == 0)
					{
						await SubTopic.Node.RemoveErrorAsync("TranducerError");
						ContainsMomentary = true;
					}
					else
						await SubTopic.Node.LogErrorAsync("TranducerError", "Transducer error: " + ErrorCode.ToString("X4"));
				}
				else
				{
					await this.LogErrorAsync("TransducerResponseError", "Unable to parse Transducer response.");
					return DataProcessingResult.Processed;
				}
			}
			else if (Message is TedsAccessMessage TedsAccessMessage)
			{
				if (TedsAccessMessage.TryParseTeds(true, out ushort ErrorCode, out Teds Teds))
				{
					await this.RemoveErrorAsync("TedsResponseError");

					StringBuilder sb = new StringBuilder();

					sb.Append(this.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Teds.ChannelInfo.NcapId));

					if (!MessageData.IsZero(Teds.ChannelInfo.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Teds.ChannelInfo.TimId));

						if (Teds.ChannelInfo.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Teds.ChannelInfo.ChannelId.ToString());
						}
					}

					SubTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (ErrorCode == 0)
						await SubTopic.Node.RemoveErrorAsync("TedsError");
					else
						await SubTopic.Node.LogErrorAsync("TedsError", "TEDS error: " + ErrorCode.ToString("X4"));
				}
				else
				{
					await this.LogErrorAsync("TedsResponseError", "Unable to parse TEDS response.");
					return DataProcessingResult.Processed;
				}
			}
			else
				return DataProcessingResult.Processed;

			if (!(SubTopic?.Node is MqttNcapTopicNode NcapTopicNode))
				return DataProcessingResult.Processed;

			if (NcapTopicNode.ResponseReceived(Topic, Message) || !ContainsMomentary)
				return DataProcessingResult.Processed;
			else
				return DataProcessingResult.ProcessedNewMomentaryValues;
		}

		private async Task ProcessRequest(Message Message)
		{
			MqttTopic SubTopic;

			if (Message is TransducerAccessMessage TransducerAccessMessage)
			{
				if (TransducerAccessMessage.TryParseRequest(out ChannelAddress Address,
					out SamplingMode SamplingMode, out double TimeoutSeconds))
				{
					await this.RemoveErrorAsync("TransducerRequestError");

					StringBuilder sb = new StringBuilder();

					sb.Append(this.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Address.NcapId));

					if (!MessageData.IsZero(Address.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Address.TimId));

						if (Address.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Address.ChannelId.ToString());
						}
					}

					SubTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (!(SubTopic?.Node is MqttNcapTopicNode NcapTopicNode))
						return;

					await NcapTopicNode.TransducerDataRequest(Message, SamplingMode, TimeoutSeconds);
				}
				else
				{
					await this.LogErrorAsync("TransducerRequestError", "Unable to parse Transducer request.");
					return;
				}
			}
			else if (Message is TedsAccessMessage TedsAccessMessage)
			{
				if (TedsAccessMessage.TryParseRequest(out ChannelAddress Address,
					out TedsAccessCode TedsAccessCode, out uint TedsOffset,
					out double TimeoutSeconds))
				{
					await this.RemoveErrorAsync("TedsRequestError");

					StringBuilder sb = new StringBuilder();

					sb.Append(this.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Address.NcapId));

					if (!MessageData.IsZero(Address.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Address.TimId));

						if (Address.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Address.ChannelId.ToString());
						}
					}

					SubTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (!(SubTopic?.Node is MqttNcapTopicNode NcapTopicNode))
						return;

					await NcapTopicNode.TedsRequest(Message, TedsAccessCode, TedsOffset, TimeoutSeconds);
				}
				else
				{
					await this.LogErrorAsync("TedsRequestError", "Unable to parse TEDS request.");
					return;
				}
			}
			else
				return;
		}

		private Task LogErrorAsync(string EventId, string Message)
		{
			return this.Topic?.Node?.LogErrorAsync(EventId, Message) ?? Task.CompletedTask;
		}

		private Task RemoveErrorAsync(string EventId)
		{
			return this.Topic?.Node?.RemoveErrorAsync(EventId) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Perfect;

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Data = new List<Field>()
			{
				new Int32Field(ThingReference, this.Timestamp, this.Append(Prefix, "#Bytes"),
					this.value?.Length ?? 0, FieldType.Momentary, FieldQoS.AutomaticReadout)
			};

			if (!(this.value is null) && this.value.Length <= 256)
			{
				Data.Add(new StringField(ThingReference, this.Timestamp, "Raw",
					Convert.ToBase64String(this.value), FieldType.Momentary, FieldQoS.AutomaticReadout));
			}

			Request.ReportFields(Last, Data);

			return Task.CompletedTask;
		}
	}
}
