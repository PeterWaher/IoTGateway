using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Mqtt;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 NCAP.
	/// </summary>
	public class MqttNcapTopicNode : MqttTopicNode
	{
		/// <summary>
		/// Topic node representing an IEEE 1451.0 NCAP.
		/// </summary>
		public MqttNcapTopicNode()
		{
		}

		/// <summary>
		/// Local ID
		/// </summary>
		public override string LocalId => this.NodeId;

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 5, "IEEE 1451.0 NCAP");
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MqttTopicNode);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is MqttTimTopicNode);
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if (Topic.CurrentParentTopic.Node is MqttTopicNode &&
				Topic.SegmentIndex == Topic.Segments.Length - 3 &&
				Guid.TryParse(Topic.CurrentSegment, out _) &&
				Guid.TryParse(Topic.Segments[Topic.SegmentIndex + 1], out _) &&
				int.TryParse(Topic.Segments[Topic.SegmentIndex + 2], out _))
			{
				return Grade.Excellent;
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
			return new MqttNcapTopicNode()
			{
				NodeId = await GetUniqueNodeId("NCAP-" + Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment
			};
		}
	}
}
