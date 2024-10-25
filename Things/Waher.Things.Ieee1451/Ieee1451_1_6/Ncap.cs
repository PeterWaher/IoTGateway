using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Metering;
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
		public Task<DataProcessingResult> DataReported(MqttTopic Topic, MqttContent Content, byte[] Data)
		{
			if (!Ieee1451Parser.TryParseMessage(Data, out Message Message))
				return Task.FromResult(DataProcessingResult.Incompatible);

			this.value = Data;
			this.Timestamp = DateTime.UtcNow;
			this.QoS = Content.Header.QualityOfService;
			this.Retain = Content.Header.Retain;

			if (Topic is null)
				return Task.FromResult(DataProcessingResult.Processed);

			return MessageReceived(this, Topic, Message);
		}

		/// <summary>
		/// Processes an IEEE 1451.0 message.
		/// </summary>
		/// <param name="This">MQTT Data object processing an IEEE 1451.0 message.</param>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Message">Parsed binary message.</param>
		/// <returns>Data processing result</returns>
		public static async Task<DataProcessingResult> MessageReceived(MqttData This, MqttTopic Topic, Message Message)
		{
			try
			{
				switch (Message.MessageType)
				{
					case MessageType.Reply:
						return await ProcessReply(This, Topic, Message);

					case MessageType.Command:
						await ProcessRequest(This, Message);
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
				await LogErrorAsync(This, string.Empty, ex.Message);
			}

			return DataProcessingResult.Processed;
		}

		internal static async Task<DataProcessingResult> ProcessReply(MqttData This, MqttTopic Topic, Message Message)
		{
			MqttTopic SubTopic;
			bool ContainsMomentary = false;

			if (Message is TransducerAccessMessage TransducerAccessMessage)
			{
				ThingReference Ref = new ThingReference(This.Topic.Node);
				if (TransducerAccessMessage.TryParseTransducerData(Ref, null, out ushort ErrorCode, out TransducerData Data))
				{
					await RemoveErrorAsync(This, "TransducerResponseError");

					SubTopic = await This.Topic.Broker.GetTopic(Data.ChannelInfo.GetTopic(This.Topic.FullTopic), true, false);

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
					await LogErrorAsync(This, "TransducerResponseError", "Unable to parse Transducer response.");
					return DataProcessingResult.Processed;
				}
			}
			else if (Message is TedsAccessMessage TedsAccessMessage)
			{
				if (TedsAccessMessage.TryParseTeds(true, out ushort ErrorCode, out Teds Teds))
				{
					await RemoveErrorAsync(This, "TedsResponseError");

					SubTopic = await This.Topic.Broker.GetTopic(Teds.ChannelInfo.GetTopic(This.Topic.FullTopic), true, false);

					if (ErrorCode == 0)
						await SubTopic.Node.RemoveErrorAsync("TedsError");
					else
						await SubTopic.Node.LogErrorAsync("TedsError", "TEDS error: " + ErrorCode.ToString("X4"));
				}
				else
				{
					await LogErrorAsync(This, "TedsResponseError", "Unable to parse TEDS response.");
					return DataProcessingResult.Processed;
				}
			}
			else if (Message is DiscoveryMessage DiscoveryMessage)
			{
				if (DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data))
				{
					await RemoveErrorAsync(This, "DiscoveryResponseError");

					string TopicString;
					bool Created;

					switch (DiscoveryMessage.DiscoveryService)
					{
						case DiscoveryService.NCAPDiscovery:
							if (Data is DiscoveryDataEntity NcapEntity)
							{
								TopicString = NcapEntity.Channel.GetTopic(This.Topic.FullTopic);

								SubTopic = await This.Topic.Broker.GetTopic(TopicString, false, false);
								if (Created = SubTopic is null)
									SubTopic = await This.Topic.Broker.GetTopic(TopicString, true, false);

								if (SubTopic.Node is MqttNcapTopicNode TopicNode)
									await TopicNode.NameReceived(NcapEntity.Name);

								if (ErrorCode == 0)
									await SubTopic.Node.RemoveErrorAsync("DiscoveryError");
								else
									await SubTopic.Node.LogErrorAsync("DiscoveryError", "Discovery error: " + ErrorCode.ToString("X4"));

								if (Created)
								{
									byte[] Request = DiscoveryMessage.SerializeRequest(NcapEntity.Channel.NcapId);
									MqttBroker Broker = Topic.Broker;
									await Broker.Publish(This.Topic.FullTopic, MqttQualityOfService.AtLeastOnce, false, Request);
								}
							}
							break;

						case DiscoveryService.NCAPTIMDiscovery:
							if (Data is DiscoveryDataEntities TimEntities)
							{
								int i, c = Math.Min(TimEntities.Names.Length, TimEntities.Identities.Length);

								for (i = 0; i < c; i++)
								{
									ChannelAddress TimAddress = new ChannelAddress()
									{
										ApplicationId = TimEntities.Channel.ApplicationId,
										NcapId = TimEntities.Channel.NcapId,
										TimId = TimEntities.Identities[i],
										ChannelId = 0
									};

									TopicString = TimAddress.GetTopic(This.Topic.FullTopic);
									SubTopic = await This.Topic.Broker.GetTopic(TopicString, false, false);
									if (Created = SubTopic is null)
										SubTopic = await This.Topic.Broker.GetTopic(TopicString, true, false);

									if (SubTopic.Node is MqttNcapTopicNode TopicNode)
										await TopicNode.NameReceived(TimEntities.Names[i]);

									if (ErrorCode == 0)
										await SubTopic.Node.RemoveErrorAsync("DiscoveryError");
									else
										await SubTopic.Node.LogErrorAsync("DiscoveryError", "Discovery error: " + ErrorCode.ToString("X4"));

									if (Created)
									{
										byte[] Request = DiscoveryMessage.SerializeRequest(TimEntities.Channel.NcapId, TimEntities.Identities[i]);
										MqttBroker Broker = Topic.Broker;
										await Broker.Publish(This.Topic.FullTopic, MqttQualityOfService.AtLeastOnce, false, Request);
									}
								}
							}
							break;

						case DiscoveryService.NCAPTIMTransducerDiscovery:
							if (Data is DiscoveryDataChannels Channels)
							{
								int i, c = Math.Min(Channels.Names.Length, Channels.Channels.Length);

								for (i = 0; i < c; i++)
								{
									ChannelAddress TimAddress = new ChannelAddress()
									{
										ApplicationId = Channels.Channel.ApplicationId,
										NcapId = Channels.Channel.NcapId,
										TimId = Channels.Channel.TimId,
										ChannelId = Channels.Channels[i],
									};

									TopicString = TimAddress.GetTopic(This.Topic.FullTopic);
									SubTopic = await This.Topic.Broker.GetTopic(TopicString, true, false);
									if (SubTopic.Node is MqttNcapTopicNode TopicNode)
										await TopicNode.NameReceived(Channels.Names[i]);

									if (ErrorCode == 0)
										await SubTopic.Node.RemoveErrorAsync("DiscoveryError");
									else
										await SubTopic.Node.LogErrorAsync("DiscoveryError", "Discovery error: " + ErrorCode.ToString("X4"));
								}
							}
							break;
					}
				}
				else
					await LogErrorAsync(This, "DiscoveryResponseError", "Unable to parse Discovery response.");

				return DataProcessingResult.Processed;
			}
			else
				return DataProcessingResult.Processed;

			if (!(SubTopic?.Node is MqttNcapTopicNode NcapTopicNode))
				return DataProcessingResult.Processed;

			if (!NcapTopicNode.ResponseReceived(Topic, Message) && ContainsMomentary)
			{
				// TODO: Report new momentary values on node.
			}

			return DataProcessingResult.Processed;
		}

		private static async Task ProcessRequest(MqttData This, Message Message)
		{
			MqttTopic SubTopic;

			if (Message is TransducerAccessMessage TransducerAccessMessage)
			{
				if (TransducerAccessMessage.TryParseRequest(out ChannelAddress Address,
					out SamplingMode SamplingMode, out double TimeoutSeconds))
				{
					await RemoveErrorAsync(This, "TransducerRequestError");

					StringBuilder sb = new StringBuilder();

					sb.Append(This.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Address.NcapId));

					if (!MessageSwitch.IsZero(Address.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Address.TimId));

						if (Address.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Address.ChannelId.ToString());
						}
					}

					SubTopic = await This.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (!(SubTopic?.Node is ProxyMqttNcapTopicNode ProxyNcapTopicNode))
						return;

					await ProxyNcapTopicNode.TransducerDataRequest(TransducerAccessMessage, SamplingMode, TimeoutSeconds);
				}
				else
				{
					await LogErrorAsync(This, "TransducerRequestError", "Unable to parse Transducer request.");
					return;
				}
			}
			else if (Message is TedsAccessMessage TedsAccessMessage)
			{
				if (TedsAccessMessage.TryParseRequest(out ChannelAddress Address,
					out TedsAccessCode TedsAccessCode, out uint TedsOffset,
					out double TimeoutSeconds))
				{
					await RemoveErrorAsync(This, "TedsRequestError");

					StringBuilder sb = new StringBuilder();

					sb.Append(This.Topic.FullTopic);
					sb.Append('/');
					sb.Append(Hashes.BinaryToString(Address.NcapId));

					if (!MessageSwitch.IsZero(Address.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Address.TimId));

						if (Address.ChannelId != 0)
						{
							sb.Append('/');
							sb.Append(Address.ChannelId.ToString());
						}
					}

					SubTopic = await This.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (!(SubTopic?.Node is ProxyMqttNcapTopicNode ProxyNcapTopicNode))
						return;

					await ProxyNcapTopicNode.TedsRequest(TedsAccessMessage, TedsAccessCode, TedsOffset, TimeoutSeconds);
				}
				else
				{
					await LogErrorAsync(This, "TedsRequestError", "Unable to parse TEDS request.");
					return;
				}
			}
			else if (Message is DiscoveryMessage DiscoveryMessage)
			{
				if (DiscoveryMessage.TryParseMessage(out ushort _, out DiscoveryData Data))
				{
					await RemoveErrorAsync(This, "DiscoveryRequestError");

					StringBuilder sb = new StringBuilder();

					sb.Append(This.Topic.FullTopic);
					if (!MessageSwitch.IsZero(Data.Channel.TimId))
					{
						sb.Append('/');
						sb.Append(Hashes.BinaryToString(Data.Channel.NcapId));

						if (!MessageSwitch.IsZero(Data.Channel.TimId))
						{
							sb.Append('/');
							sb.Append(Hashes.BinaryToString(Data.Channel.TimId));

							if (Data.Channel.ChannelId != 0)
							{
								sb.Append('/');
								sb.Append(Data.Channel.ChannelId.ToString());
							}
						}
					}

					SubTopic = await This.Topic.Broker.GetTopic(sb.ToString(), true, false);

					if (SubTopic?.Node is MeteringNode Node)
					{
						bool Broadcast = SubTopic.LocalTopic == "D0" &&
							await Node.GetParent() is RootTopic;

						LinkedList<INode> ToProcess = new LinkedList<INode>();
						IEnumerable<INode> ChildNodes = await Node.ChildNodes;
						if (!(ChildNodes is null))
						{
							foreach (INode ChildNode in ChildNodes)
								ToProcess.AddLast(ChildNode);
						}

						while (!(ToProcess.First is null))
						{
							INode ChildNode = ToProcess.First.Value;
							ToProcess.RemoveFirst();

							bool CheckChildren;

							if (ChildNode is ProxyMqttNcapTopicNode ProxyNcapTopicNode)
							{
								await ProxyNcapTopicNode.DiscoveryRequest(DiscoveryMessage, Data);
								CheckChildren = Broadcast;
							}
							else if (ChildNode is DiscoverableTopicNode)
								CheckChildren = Broadcast;
							else
								CheckChildren = false;

							if (CheckChildren)
							{
								ChildNodes = await ChildNode.ChildNodes;
								if (!(ChildNodes is null))
								{
									foreach (INode ChildNode2 in ChildNodes)
										ToProcess.AddLast(ChildNode2);
								}
							}
						}
					}
				}
				else
				{
					await LogErrorAsync(This, "DiscoveryRequestError", "Unable to parse TEDS request.");
					return;
				}
			}
		}

		private static Task LogErrorAsync(MqttData This, string EventId, string Message)
		{
			return This.Topic?.Node?.LogErrorAsync(EventId, Message) ?? Task.CompletedTask;
		}

		private static Task RemoveErrorAsync(MqttData This, string EventId)
		{
			return This.Topic?.Node?.RemoveErrorAsync(EventId) ?? Task.CompletedTask;
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
