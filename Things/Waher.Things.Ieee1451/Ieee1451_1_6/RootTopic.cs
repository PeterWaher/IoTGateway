using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Mqtt;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// IEEE 1451.1.6 root topic node
	/// </summary>
	public class RootTopic : MqttTopicNode
	{
		/// <summary>
		/// IEEE 1451.1.6 root topic node
		/// </summary>
		public RootTopic()
		{
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if (!(Topic.CurrentParentTopic is null))
				return Grade.NotAtAll;

			switch (Topic.CurrentSegment)
			{
				case "璑":   // Unicode character representing the bytes 145, 116
				case "1451.1.6":
				case "_1451.1.6":
					return Grade.Perfect;

				default:
					return Grade.NotAtAll;

			}
		}

		/// <summary>
		/// Creates a new node of the same type.
		/// </summary>
		/// <param name="Topic">MQTT Topic being processed.</param>
		/// <returns>New node instance.</returns>
		public override async Task<IMqttTopicNode> CreateNew(MqttTopicRepresentation Topic)
		{
			return new RootTopic()
			{
				NodeId = await GetUniqueNodeId(Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment
			};
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is IMqttTopicNode);
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MqttBrokerNode);
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(RootTopic), 11, "IEEE 1451.1.6 Root");
		}

	}
}
