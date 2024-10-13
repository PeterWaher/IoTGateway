using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model.Encapsulations;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 NCAP.
	/// </summary>
	public class MqttNcapTopicNode : MqttTopicNode
	{
		private string ncapId;

		/// <summary>
		/// Topic node representing an IEEE 1451.0 NCAP.
		/// </summary>
		public MqttNcapTopicNode()
		{
		}

		/// <summary>
		/// NCAP ID
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(2, "NCAP ID:")]
		[ToolTip(3, "NCAP unique identifier.")]
		[Required]
		[RegularExpression("[A-Fa-f0-9]{32}")]
		public string NcapId
		{
			get => this.ncapId;
			set => this.ncapId = value;
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
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Parameters = (LinkedList<Parameter>)await base.GetDisplayableParametersAsync(Language, Caller);

			Parameters.AddLast(new StringParameter("NcapId", 
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 4, "NCAP ID"), 
				this.ncapId));

			return Parameters;
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
				LocalTopic = Topic.CurrentSegment,
				NcapId = Topic.CurrentSegment
			};
		}
	}
}
