using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing a proxy for an IEEE 1451.0 Channel.
	/// </summary>
	public class ProxyMqttChannelTopicNode : MqttChannelTopicNode, ITransducerNode, ITedsNode
	{
		private string proxyNodeId;
		private string proxyFieldName;

		/// <summary>
		/// Topic node representing a proxy for an IEEE 1451.0 Channel.
		/// </summary>
		public ProxyMqttChannelTopicNode()
		{
		}

		/// <summary>
		/// Proxy Node ID
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(29, "Proxy Node ID:", 400)]
		[ToolTip(30, "Node ID of device to represent using this channel node.")]
		[Required]
		public string ProxyNodeId
		{
			get => this.proxyNodeId;
			set => this.proxyNodeId = value;
		}

		/// <summary>
		/// Proxy Field Name
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(31, "Proxy Field Name:", 500)]
		[ToolTip(32, "Name of field to represent using this channel node.")]
		[Required]
		public string ProxyFieldName
		{
			get => this.proxyFieldName;
			set => this.proxyFieldName = value;
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
			return Language.GetStringAsync(typeof(MqttNcapTopicNode), 33, "IEEE 1451.0 Proxy Channel");
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

			Parameters.AddLast(new StringParameter("ProxyNodeId",
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 34, "Proxy Node ID"),
				this.ProxyNodeId));

			Parameters.AddLast(new StringParameter("ProxyFieldName",
				await Language.GetStringAsync(typeof(MqttNcapTopicNode), 35, "Proxy Field Name"),
				this.ProxyFieldName));

			return Parameters;
		}

		/// <summary>
		/// If the node accepts a given parent.
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ProxyMqttTimTopicNode);
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
				Topic.CurrentParentTopic.Node is MqttTimTopicNode &&
				!(Topic.CurrentParentTopic.Node is MqttChannelTopicNode) &&
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

		/// <summary>
		/// A request for transducer data has been received.
		/// </summary>
		/// <param name="TransducerAccessMessage">Message</param>
		/// <param name="SamplingMode">Sampling mode.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public Task TransducerDataRequest(TransducerAccessMessage TransducerAccessMessage,
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
		public Task TedsRequest(TedsAccessMessage TedsAccessMessage,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds)
		{
			return Task.CompletedTask;  // TODO
		}

	}
}
