﻿using System;
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
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 5, "IEEE 1451.0 NCAP");
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if (Topic.SegmentIndex != Topic.Segments.Length - 3 ||
				!Guid.TryParse(Topic.CurrentSegment, out _) ||
				!Guid.TryParse(Topic.Segments[Topic.SegmentIndex + 1], out _) ||
				!int.TryParse(Topic.Segments[Topic.SegmentIndex + 2], out _))
			{
				return Grade.NotAtAll;
			}

			return Grade.Excellent;
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
				NodeId = await GetUniqueNodeId(Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment
			};
		}
	}
}
