using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0.Model;
using Waher.Things.Mqtt.Model;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Abstract base class for IEEE 1451.1.6 NCAPs.
	/// </summary>
	public abstract class Ncap : MqttData, IClient
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
		public async Task<bool> DataReported(MqttTopic Topic, MqttContent Content, byte[] Data)
		{
			if (!Ieee1451Parser.TryParseMessage(Data, out Message Message))
				return false;

			this.value = Data;
			this.Timestamp = DateTime.UtcNow;
			this.QoS = Content.Header.QualityOfService;
			this.Retain = Content.Header.Retain;

			if (Topic is null)
				return true;

			await this.MessageReceived(Topic, Message);

			return true;
		}

		/// <summary>
		/// Processes an IEEE 1451.0 message.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Message">Parsed binary message.</param>
		public virtual async Task MessageReceived(MqttTopic Topic, Message Message)
		{
			try
			{
				await Message.ProcessIncoming(this);
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync(string.Empty, ex.Message);
			}
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

		private async Task<MqttTopic> GetChannelTopic(TransducerAccessMessage Message)
		{
			ThingReference Ref = new ThingReference(this.Topic.Node);
			if (Message.TryParseTransducerData(Ref, out ushort ErrorCode, out TransducerData Data))
			{
				await this.RemoveErrorAsync("TransducerDataError");

				StringBuilder sb = new StringBuilder();

				sb.Append(this.Topic.FullTopic);
				sb.Append('/');
				sb.Append(Hashes.BinaryToString(Data.ChannelInfo.NcapId));
				sb.Append('/');
				sb.Append(Hashes.BinaryToString(Data.ChannelInfo.TimId));
				sb.Append('/');
				sb.Append(Data.ChannelInfo.ChannelId.ToString());

				MqttTopic ChannelTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

				if (ErrorCode == 0)
					await ChannelTopic.Node.RemoveErrorAsync("TranducerError");
				else
					await ChannelTopic.Node.LogErrorAsync("TranducerError", "Transducer error: " + ErrorCode.ToString("X4"));

				return ChannelTopic;
			}
			else
			{
				await this.LogErrorAsync("TransducerDataError", "Unable to parse Transducer data.");
				return null;
			}
		}

		private async Task<MqttTopic> GetChannelTopic(TedsAccessMessage Message)
		{
			if (Message.TryParseTeds(true, out ushort ErrorCode, out Teds Teds))
			{
				await this.RemoveErrorAsync("TedsDataError");

				StringBuilder sb = new StringBuilder();

				sb.Append(this.Topic.FullTopic);
				sb.Append('/');
				sb.Append(Hashes.BinaryToString(Teds.ChannelInfo.NcapId));
				sb.Append('/');
				sb.Append(Hashes.BinaryToString(Teds.ChannelInfo.TimId));
				sb.Append('/');
				sb.Append(Teds.ChannelInfo.ChannelId.ToString());

				MqttTopic ChannelTopic = await this.Topic.Broker.GetTopic(sb.ToString(), true, false);

				if (ErrorCode == 0)
					await ChannelTopic.Node.RemoveErrorAsync("TedsError");
				else
					await ChannelTopic.Node.LogErrorAsync("TedsError", "TEDS error: " + ErrorCode.ToString("X4"));

				return ChannelTopic;
			}
			else
			{
				await this.LogErrorAsync("TedsDataError", "Unable to parse TEDS data.");
				return null;
			}
		}

		/// <summary>
		/// A transducer access command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TransducerAccessCommand(TransducerAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TransducerAccessCommand(Message);
		}

		/// <summary>
		/// A transducer access reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TransducerAccessReply(TransducerAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TransducerAccessReply(Message);
		}

		/// <summary>
		/// A transducer access announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TransducerAccessAnnouncement(TransducerAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TransducerAccessAnnouncement(Message);
		}

		/// <summary>
		/// A transducer access notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TransducerAccessNotification(TransducerAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TransducerAccessNotification(Message);
		}

		/// <summary>
		/// A transducer access callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TransducerAccessCallback(TransducerAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TransducerAccessCallback(Message);
		}

		/// <summary>
		/// A TEDS access command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TedsAccessCommand(TedsAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TedsAccessCommand(Message);
		}

		/// <summary>
		/// A TEDS access reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TedsAccessReply(TedsAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TedsAccessReply(Message);
		}

		/// <summary>
		/// A TEDS access announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TedsAccessAnnouncement(TedsAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TedsAccessAnnouncement(Message);
		}

		/// <summary>
		/// A TEDS access notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TedsAccessNotification(TedsAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TedsAccessNotification(Message);
		}

		/// <summary>
		/// A TEDS access callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public async Task TedsAccessCallback(TedsAccessMessage Message)
		{
			MqttTopic Channel = await this.GetChannelTopic(Message);
			if (!(Channel?.Node is MqttChannelTopicNode ChannelTopicNode))
				return;

			await ChannelTopicNode.TedsAccessCallback(Message);
		}

		/// <summary>
		/// A discovery command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task DiscoveryCommand(DiscoveryMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// A discovery reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task DiscoveryReply(DiscoveryMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// A discovery announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task DiscoveryAnnouncement(DiscoveryMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// A discovery notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task DiscoveryNotification(DiscoveryMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// A discovery callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task DiscoveryCallback(DiscoveryMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// An Events notification command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task EventNotificationCommand(EventNotificationMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// An Events notification reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task EventNotificationReply(EventNotificationMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// An Events notification announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task EventNotificationAnnouncement(EventNotificationMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// An Events notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task EventNotificationNotification(EventNotificationMessage Message) { return Task.CompletedTask; }

		/// <summary>
		/// An Events notification callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		public Task EventNotificationCallback(EventNotificationMessage Message) { return Task.CompletedTask; }

	}
}
