using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Security;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Topic node representing an IEEE 1451.0 NCAP.
	/// </summary>
	public class MqttNcapTopicNode : MqttTopicNode
	{
		private string entityName;
		private string ncapId;
		private byte[] ncapIdBin;
		private int timeoutMilliseconds = 10000;
		private int staleSeconds = 60;
		private int refreshTedsHours = 24;

		/// <summary>
		/// Topic node representing an IEEE 1451.0 NCAP.
		/// </summary>
		public MqttNcapTopicNode()
		{
		}

		/// <summary>
		/// Name
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(25, "Entity Name:", 50)]
		[ToolTip(26, "Name of entity, as configured in the device.")]
		public string EntityName
		{
			get => this.entityName;
			set => this.entityName = value;
		}

		/// <summary>
		/// NCAP ID
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(2, "NCAP ID:", 100)]
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
		/// Timeout for request/response, in milliseconds.
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(16, "Timeout: (ms)", 1000)]
		[ToolTip(17, "Maximum amount of time to wait (in milliseconds) for a response to a request.")]
		[Required]
		[Range(1, int.MaxValue)]
		public int TimeoutMilliseconds
		{
			get => this.timeoutMilliseconds;
			set => this.timeoutMilliseconds = value;
		}

		/// <summary>
		/// Timeout for request/response, in milliseconds.
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(18, "Stale after: (s)", 2000)]
		[ToolTip(19, "Flags information as stale (old) after this amount of time, triggering new requests if information is requested.")]
		[Required]
		[Range(1, int.MaxValue)]
		public int StaleSeconds
		{
			get => this.staleSeconds;
			set => this.staleSeconds = value;
		}

		/// <summary>
		/// Refresh TEDS if older than this number of hours.
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(27, "Refresh TEDS: (h)", 3000)]
		[ToolTip(28, "Re-fetches TEDS from device, if current TEDS is older than this number of hours.")]
		[Required]
		[Range(1, int.MaxValue)]
		public int RefreshTedsHours
		{
			get => this.refreshTedsHours;
			set => this.refreshTedsHours = value;
		}

		/// <summary>
		/// NCAP ID in binary form.
		/// </summary>
		public byte[] NcapIdBinary => this.ncapIdBin;

		/// <summary>
		/// Local ID
		/// </summary>
		public override string LocalId
		{
			get
			{
				if (!string.IsNullOrEmpty(this.entityName))
					return this.entityName;
				else
					return this.NodeId;
			}
		}

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
			return Task.FromResult(Parent is DiscoverableTopicNode);
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
		/// <returns>If response to a pending request was received (true)</returns>
		public bool ResponseReceived(MqttTopic Topic, Ieee1451_0.Messages.Message Message)
		{
			if (Topic.Data is MessageData Data)
				return Data.DataReported(Message);
			else
			{
				Topic.SetData(new MessageData(Topic, Message, this.NcapIdBinary,
					(this as MqttTimTopicNode)?.TimIdBinary,
					(ushort)((this as MqttChannelTopicNode)?.ChannelId ?? 0)));

				return false;
			}
		}

		/// <summary>
		/// Name has been received
		/// </summary>
		/// <param name="Name">Name</param>
		public async Task NameReceived(string Name)
		{
			bool Changed = false;

			if (this.entityName != Name)
			{
				this.entityName = Name;
				Changed = true;
			}

			if (string.IsNullOrEmpty(this.Name))
			{
				this.Name = Name;
				Changed = true;
			}

			if (Changed)
				await this.NodeUpdated();
		}

	}
}
