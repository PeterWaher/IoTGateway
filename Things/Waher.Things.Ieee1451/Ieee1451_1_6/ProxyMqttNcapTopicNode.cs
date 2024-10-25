using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Mqtt;
using Waher.Networking.MQTT;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 NCAP Proxy.
	/// </summary>
	public class ProxyMqttNcapTopicNode : MqttNcapTopicNode
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
		/// A request for transducer data has been received.
		/// </summary>
		/// <param name="TransducerAccessMessage">Message</param>
		/// <param name="SamplingMode">Sampling mode.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public virtual Task TransducerDataRequest(TransducerAccessMessage TransducerAccessMessage,
			SamplingMode SamplingMode, double TimeoutSeconds)
		{
			return Task.CompletedTask;  // TODO
		}

		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="TedsAccessMessage">Message</param>
		/// <param name="TedsAccessCode">TEDS access code.</param>
		/// <param name="TedsOffset">TEDS offset.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public virtual Task TedsRequest(TedsAccessMessage TedsAccessMessage,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds)
		{
			return Task.CompletedTask;  // TODO
		}

		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="DiscoveryMessage">Message</param>
		/// <param name="Data">Discovery data in request.</param>
		public virtual async Task DiscoveryRequest(DiscoveryMessage DiscoveryMessage, DiscoveryData Data)
		{
			if (DiscoveryMessage.DiscoveryService == DiscoveryService.NCAPDiscovery)
			{
				if (!(await this.GetParent() is DiscoverableTopicNode Parent))
					return;

				MqttBrokerNode Broker = await this.GetBroker();
				if (Broker is null)
					return;

				byte[] Response = DiscoveryMessage.SerializeResponse(0, this.NcapIdBinary, this.EntityName);

				string Topic = await Parent.GetFullTopic();
				await Broker.GetBroker().Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
			}
		}
	}
}
