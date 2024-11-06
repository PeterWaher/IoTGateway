using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.MetaTeds;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 TIM Proxy.
	/// </summary>
	public class ProxyMqttTimTopicNode : MqttTimTopicNode, IDiscoverableNode, ITedsNode
	{
		/// <summary>
		/// Topic node representing an IEEE 1451.0 TIM Proxy.
		/// </summary>
		public ProxyMqttTimTopicNode()
		{
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 1, "IEEE 1451.0 Proxy TIM");
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ProxyMqttNcapTopicNode);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ProxyMqttChannelTopicNode);
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			return Grade.NotAtAll;
		}

		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="DiscoveryMessage">Message</param>
		public async Task DiscoveryRequest(DiscoveryMessage DiscoveryMessage)
		{
			if (!(await this.GetParent() is ProxyMqttNcapTopicNode Parent))
				return;

			if (!(await Parent.GetParent() is DiscoverableTopicNode GrandParent))
				return;

			MqttBrokerNode BrokerNode = await GrandParent.GetBroker();
			if (BrokerNode is null)
				return;

			MqttBroker Broker = await BrokerNode.GetBroker();
			if (Broker is null)
				return;

			string Topic = await GrandParent.GetFullTopic();
			if (string.IsNullOrEmpty(Topic))
				return;

			switch (DiscoveryMessage.DiscoveryService)
			{
				case DiscoveryService.NCAPTIMTransducerDiscovery:
					List<ushort> ChannelIds = new List<ushort>();
					List<string> Names = new List<string>();

					foreach (INode Child in await this.ChildNodes)
					{
						if (!(Child is ProxyMqttChannelTopicNode ChannelNode))
							continue;

						ChannelIds.Add((ushort)ChannelNode.ChannelId);
						Names.Add(ChannelNode.EntityName);
					}

					if (ChannelIds.Count == 0)
						break;

					StringBuilder ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					byte[] Response = DiscoveryMessage.SerializeResponse(0, this.NcapIdBinary, this.TimIdBinary, ChannelIds.ToArray(), Names.ToArray(), ToSniffer);

					if (!(ToSniffer is null))
						await BrokerNode.Information(ToSniffer.ToString());

					await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
					break;
			}
		}

		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="TedsAccessMessage">Message</param>
		/// <param name="TedsAccessCode">TEDS access code.</param>
		/// <param name="TedsOffset">TEDS offset.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public async Task TedsRequest(TedsAccessMessage TedsAccessMessage,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds)
		{
			if (!(await this.GetParent() is ProxyMqttNcapTopicNode NcapNode))
				return;

			if (!(await NcapNode.GetParent() is DiscoverableTopicNode CommunicationNode))
				return;

			MqttBrokerNode BrokerNode = await CommunicationNode.GetBroker();
			if (BrokerNode is null)
				return;

			MqttBroker Broker = await BrokerNode.GetBroker();
			if (Broker is null)
				return;

			string Topic = await CommunicationNode.GetFullTopic();
			if (string.IsNullOrEmpty(Topic))
				return;

			byte[] Response;
			StringBuilder ToSniffer;

			switch (TedsAccessCode)
			{
				case TedsAccessCode.MetaTEDS:
					ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					int NrTransducerChannels = 0;

					foreach (INode Child in await this.ChildNodes)
					{
						if (Child is ProxyMqttChannelTopicNode)
							NrTransducerChannels++;
					}

					Response = TedsAccessMessage.SerializeResponse(0, this.NcapIdBinary,
						this.TimIdBinary, 0, ToSniffer,
						new TedsId(99, 255, (byte)TedsAccessCode.MetaTEDS, 2, 1),
						new Uuid(this.TimIdBinary),
						new NrTransducerChannels(NrTransducerChannels));
					break;

				case TedsAccessCode.XdcrName:
					ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					Response = TedsAccessMessage.SerializeResponse(0, this.NcapIdBinary,
						this.TimIdBinary, 0, ToSniffer,
						new TedsId(99, 255, (byte)TedsAccessCode.XdcrName, 2, 1),
						new Format(true),
						new Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds.Content(this.EntityName));
					break;

				default:
					return;
			}

			if (!(ToSniffer is null))
				await BrokerNode.Information(ToSniffer.ToString());

			await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
		}
	}
}
