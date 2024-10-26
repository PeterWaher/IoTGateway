using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 TIM Proxy.
	/// </summary>
	public class ProxyMqttTimTopicNode : MqttTimTopicNode
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

			MqttBroker Broker = BrokerNode.GetBroker();
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

					byte[] Response = DiscoveryMessage.SerializeResponse(0, this.NcapIdBinary, this.TimIdBinary, ChannelIds.ToArray(), Names.ToArray());
					await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
					break;
			}
		}
	}
}
