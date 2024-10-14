using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Security;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Metering;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 NCAP.
	/// </summary>
	public class MqttNcapTopicNode : MqttTopicNode
	{
		private string ncapId;
		private byte[] ncapIdBin;

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
			set
			{
				this.ncapIdBin = Hashes.StringToBinary(value);
				this.ncapId = value;
			}
		}

		/// <summary>
		/// NCAP ID in binary form.
		/// </summary>
		public byte[] NcapIdBinary => this.ncapIdBin;

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
				!(Topic.CurrentParentTopic.Node is MqttNcapTopicNode) &&
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
			return new MqttNcapTopicNode()
			{
				NodeId = await GetUniqueNodeId("NCAP-" + Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment,
				NcapId = Topic.CurrentSegment
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

			return new MessageData(Topic, this.ncapIdBin,
				(this as MqttTimTopicNode)?.TimIdBinary,
				(ushort)((this as MqttChannelTopicNode)?.ChannelId ?? 0));
		}

		/// <summary>
		/// A response message has been received.
		/// </summary>
		/// <param name="Topic">MQTT topic.</param>
		/// <param name="Message">Message</param>
		public void ResponseReceived(MqttTopic Topic, Ieee1451_0.Messages.Message Message)
		{
			if (Topic.Data is MessageData Data)
				Data.DataReported(Message);
			else
			{
				Topic.SetData(new MessageData(Topic, Message, this.NcapIdBinary,
					(this as MqttTimTopicNode)?.TimIdBinary,
					(ushort)((this as MqttChannelTopicNode)?.ChannelId ?? 0)));
			}
		}

		/// <summary>
		/// A request for transducer data has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="SamplingMode">Sampling mode.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public Task TransducerDataRequest(Ieee1451_0.Messages.Message Message,
			SamplingMode SamplingMode, double TimeoutSeconds)
		{
			return Task.CompletedTask;	// TODO
		}

		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="TedsAccessCode">TEDS access code.</param>
		/// <param name="TedsOffset">TEDS offset.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		public Task TedsRequest(Ieee1451_0.Messages.Message Message,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds)
		{
			return Task.CompletedTask;  // TODO
		}

	}
}
