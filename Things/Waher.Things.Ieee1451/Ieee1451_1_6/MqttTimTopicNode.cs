using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Mqtt;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 TIM.
	/// </summary>
	public class MqttTimTopicNode : MqttNcapTopicNode
	{
		private string timId;

		/// <summary>
		/// Topic node representing an IEEE 1451.0 TIM.
		/// </summary>
		public MqttTimTopicNode()
		{
		}

		/// <summary>
		/// TIM ID
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(5, "TIM ID:")]
		[ToolTip(6, "TIM unique identifier.")]
		[Required]
		[RegularExpression("[A-Fa-f0-9]{32}")]
		public string TimId
		{
			get => this.timId;
			set => this.timId = value;
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
			return Language.GetStringAsync(typeof(Ieee1451Parser), 6, "IEEE 1451.0 TIM");
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

			Parameters.AddLast(new StringParameter("TimId",
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 7, "TIM ID"),
				this.TimId));

			Parameters.AddLast(new StringParameter("NcapId",
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 4, "NCAP ID"),
				this.NcapId));

			return Parameters;
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MqttNcapTopicNode);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is MqttChannelTopicNode);
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if (Topic.CurrentParentTopic.Node is MqttNcapTopicNode &&
				!(Topic.CurrentParentTopic.Node is MqttTimTopicNode) &&
				Guid.TryParse(Topic.CurrentSegment, out _))
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
			return new MqttTimTopicNode()
			{
				NodeId = await GetUniqueNodeId("TIM-" + Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment,
				TimId = Topic.CurrentSegment,
				NcapId = Topic.Segments[Topic.SegmentIndex - 1]
			};
		}
	}
}
