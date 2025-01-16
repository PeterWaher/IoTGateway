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
	/// Topic node representing an IEEE 1451.0 NCAP Proxy.
	/// </summary>
	public class ProxyMqttNcapTopicNode : MqttNcapTopicNode, IDiscoverableNode, ITedsNode
	{
		/// <summary>
		/// Topic node representing an IEEE 1451.0 NCAP Proxy.
		/// </summary>
		public ProxyMqttNcapTopicNode()
		{
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 8, "IEEE 1451.0 Proxy NCAP");
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is DiscoverableTopicNode);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ProxyMqttTimTopicNode);
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
			if (!(await this.GetParent() is DiscoverableTopicNode Parent))
				return;

			MqttBrokerNode BrokerNode = await Parent.GetBroker();
			if (BrokerNode is null)
				return;

			MqttBroker Broker = await BrokerNode.GetBroker();
			if (Broker is null)
				return;

			string Topic = await Parent.GetFullTopic();
			if (string.IsNullOrEmpty(Topic))
				return;

			switch (DiscoveryMessage.DiscoveryService)
			{
				case DiscoveryService.NCAPDiscovery:
					StringBuilder ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					byte[] Response = DiscoveryMessage.SerializeResponse(0, this.NcapIdBinary, this.EntityName, ToSniffer);

					if (!(ToSniffer is null))
						BrokerNode.Information(ToSniffer.ToString());

					await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
					break;

				case DiscoveryService.NCAPTIMDiscovery:
					List<byte[]> TimIds = new List<byte[]>();
					List<string> Names = new List<string>();

					foreach (INode Child in await this.ChildNodes)
					{
						if (!(Child is ProxyMqttTimTopicNode TimNode))
							continue;

						TimIds.Add(TimNode.TimIdBinary);
						Names.Add(TimNode.EntityName);
					}

					if (TimIds.Count == 0)
						break;

					ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					Response = DiscoveryMessage.SerializeResponse(0, this.NcapIdBinary, TimIds.ToArray(), Names.ToArray(), ToSniffer);

					if (!(ToSniffer is null))
						BrokerNode.Information(ToSniffer.ToString());

					await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
					break;
			}
		}

		/// <summary>
		/// Name has been received
		/// </summary>
		/// <param name="Name">Name</param>
		public override Task NameReceived(string Name)
		{
			return Task.CompletedTask;	// Name controlled from broker, not from external sources.
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
			if (!(await this.GetParent() is DiscoverableTopicNode CommunicationNode))
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
					Response = TedsAccessMessage.SerializeResponse(0, this.NcapIdBinary, null, 0, ToSniffer,
						new TedsId(99, 255, (byte)TedsAccessCode.MetaTEDS, 2, 1),
						new Uuid(this.NcapIdBinary));
					break;

				case TedsAccessCode.XdcrName:
					ToSniffer = BrokerNode.HasSniffers ? new StringBuilder() : null;
					Response = TedsAccessMessage.SerializeResponse(0, this.NcapIdBinary, null, 0, ToSniffer,
						new TedsId(99, 255, (byte)TedsAccessCode.XdcrName, 2, 1),
						new Format(true),
						new Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds.Content(this.EntityName));
					break;

				default:
					return;
			}

			if (!(ToSniffer is null))
				BrokerNode.Information(ToSniffer.ToString());

			await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
		}

	}
}
