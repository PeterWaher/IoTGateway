using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// MQTT Topic node that publishes discovery commands in accordance with IEEE 1451.0.
	/// </summary>
	public class DiscoverableTopicNode : MqttTopicNode
	{
		/// <summary>
		/// MQTT Topic node that publishes discovery commands in accordance with IEEE 1451.0.
		/// </summary>
		public DiscoverableTopicNode()
		{
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(RootTopic), 23, "IEEE 1451.1.6 Topic Node");
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is DiscoverableTopicNode || Parent is RootTopic);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is DiscoverableTopicNode || Child is MqttNcapTopicNode);
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if ((Topic.CurrentParentTopic.Node is RootTopic ||
				Topic.CurrentParentTopic.Node is DiscoverableTopicNode) &&
				!(Topic.CurrentParentTopic.Node is MqttNcapTopicNode))
			{
				return Grade.Ok;
			}
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Creates a new node of the same type.
		/// </summary>
		/// <param name="Topic">MQTT Topic being processed.</param>
		/// <returns>New node instance.</returns>
		public override async Task<IMqttTopicNode> CreateNew(MqttTopicRepresentation Topic)
		{
			return new DiscoverableTopicNode()
			{
				NodeId = await GetUniqueNodeId(Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment
			};
		}

		/// <summary>
		/// Gets the default data object, if any.
		/// </summary>
		/// <returns>Default data object, if one exists, or null otherwise.</returns>
		public override async Task<IMqttData> GetDefaultDataObject()
		{
			MqttTopic Topic = await this.GetTopic();
			if (Topic is null)
				return null;

			return new MessageData(Topic, null, null, 0);
		}

		/// <summary>
		/// A response message has been received.
		/// </summary>
		/// <param name="Topic">MQTT topic.</param>
		/// <param name="Message">Message</param>
		/// <returns>If response to a pending request was received (true)</returns>
		public bool ResponseReceived(MqttTopic Topic, Ieee1451_0.Messages.Message Message)
		{
			if (Topic.Data is MessageData Data)
				return Data.DataReported(Message);
			else
			{
				Topic.SetData(new MessageData(Topic, Message, null, null, 0));
				return false;
			}
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Commands = new List<ICommand>();
			Commands.AddRange(await base.Commands);

			Commands.Add(new DiscoverNcapsTopic(await this.GetBroker(), this));

			return Commands.ToArray();
		}

	}
}
