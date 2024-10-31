using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Mqtt;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 Channel.
	/// </summary>
	public class MqttChannelTopicNode : MqttTimTopicNode
	{
		private int channelId;

		/// <summary>
		/// Topic node representing an IEEE 1451.0 Channel.
		/// </summary>
		public MqttChannelTopicNode()
		{
		}

		/// <summary>
		/// Channel ID
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(8, "Channel:", 300)]
		[ToolTip(9, "Channel identifier on TIM.")]
		[Required]
		[Range(1, ushort.MaxValue)]
		public int ChannelId
		{
			get => this.channelId;
			set => this.channelId = value;
		}

		/// <summary>
		/// Local ID
		/// </summary>
		public override string LocalId
		{
			get
			{
				if (!string.IsNullOrEmpty(this.EntityName))
					return this.EntityName;
				else
					return this.NodeId;
			}
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Ieee1451Parser), 7, "IEEE 1451.0 Channel");
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

			Parameters.AddLast(new Int32Parameter("ChannelId",
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 10, "Channel"),
				this.ChannelId));

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
			return Task.FromResult(Parent is MqttTimTopicNode);
		}

		/// <summary>
		/// If the node accepts a given child.
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public override Grade Supports(MqttTopicRepresentation Topic)
		{
			if (Topic.SegmentIndex > 0 &&
				Topic.CurrentParentTopic?.Node is MqttTimTopicNode &&
				!(Topic.CurrentParentTopic?.Node is MqttChannelTopicNode) &&
				int.TryParse(Topic.CurrentSegment, out _))
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
			StringBuilder sb = new StringBuilder();

			sb.Append("Channel-");
			sb.Append(Topic.Segments[Topic.SegmentIndex - 1]);
			sb.Append('#');
			sb.Append(Topic.CurrentSegment);

			return new MqttChannelTopicNode()
			{
				NodeId = await GetUniqueNodeId(sb.ToString()),
				LocalTopic = Topic.CurrentSegment,
				ChannelId = int.Parse(Topic.CurrentSegment),
				TimId = Topic.Segments[Topic.SegmentIndex - 1],
				NcapId = Topic.Segments[Topic.SegmentIndex - 2]
			};
		}

	}
}
