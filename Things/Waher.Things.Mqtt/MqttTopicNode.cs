using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Persistence.Attributes;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// A Metering node representing an MQTT topic
	/// </summary>
	public class MqttTopicNode : ProvisionedMeteringNode, ISensor, IActuator, IMqttTopicNode
	{
		private string localTopic = string.Empty;

		/// <summary>
		/// A Metering node representing an MQTT topic
		/// </summary>
		public MqttTopicNode()
			: base()
		{
		}

		/// <summary>
		/// Local Topic segment
		/// </summary>
		[Page(2, "MQTT")]
		[Header(22, "Local Topic:")]
		[ToolTip(23, "Local topic, unique among siblings.")]
		[DefaultValueStringEmpty]
		public string LocalTopic
		{
			get => this.localTopic;
			set => this.localTopic = value;
		}

		/// <summary>
		/// Local ID
		/// </summary>
		public override string LocalId => this.localTopic;

		/// <summary>
		/// Full topic string.
		/// </summary>
		[IgnoreMember]
		public string FullTopic
		{
			get
			{
				string Result = this.localTopic;

				IMqttTopicNode Loop = this.Parent as IMqttTopicNode;
				while (!(Loop is null))
				{
					Result = Loop.LocalTopic + "/" + Result;
					Loop = Loop.Parent as IMqttTopicNode;
				}

				return Result;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		[IgnoreMember]
		public MqttBrokerNode Broker
		{
			get
			{
				INode Parent = this.Parent;

				while (!(Parent is null))
				{
					if (Parent is MqttBrokerNode Broker)
						return Broker;

					Parent = Parent.Parent;
				}

				return null;
			}
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
			return Task.FromResult(Parent is IMqttTopicNode || Parent is MqttBrokerNode);
		}

		/// <summary>
		/// Diaplayable type name for node.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IMqttTopicNode), 24, "Topic");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async Task<MqttTopic> GetTopic()
		{
			MqttBroker Broker = this.Broker?.GetBroker();
			if (Broker is null)
				return null;
			else
				return await Broker.GetTopic(this.FullTopic, false, false);
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
			MqttTopic Topic = await this.GetTopic();

			if (!(Topic is null))
				await Topic.GetDisplayableParametersAsync(Parameters, Language, Caller);

			return Parameters;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async override Task<bool> RemoveAsync(INode Child)
		{
			if (Child is IMqttTopicNode Topic)
				(await this.GetTopic())?.Remove(Topic.LocalTopic);

			return await base.RemoveAsync(Child);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult(this.GetTopic()?.Result?.GetControlParameters() ?? new ControlParameter[0]);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public virtual async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				MqttTopic Topic = await this.GetTopic();
				Topic?.StartReadout(Request);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		/// <summary>
		/// How well the topic node supports an MQTT topic
		/// </summary>
		/// <param name="Topic">Topic being processed.</param>
		/// <returns>How well the node supports a given topic, from the segment index presented.</returns>
		public virtual Grade Supports(MqttTopicRepresentation Topic)
		{
			return Grade.Barely;
		}

		/// <summary>
		/// Creates a new node of the same type.
		/// </summary>
		/// <param name="Topic">MQTT Topic being processed.</param>
		/// <returns>New node instance.</returns>
		public virtual async Task<IMqttTopicNode> CreateNew(MqttTopicRepresentation Topic)
		{
			return new MqttTopicNode()
			{
				NodeId = await GetUniqueNodeId(Topic.CurrentSegment),
				LocalTopic = Topic.CurrentSegment
			};
		}

	}
}
