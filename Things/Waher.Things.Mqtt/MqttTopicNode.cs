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
using Waher.Things.Virtual;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// A Metering node representing an MQTT topic
	/// </summary>
	public class MqttTopicNode : VirtualNode, ISensor, IActuator, IMqttTopicNode
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
		/// Gets the full topic string.
		/// </summary>
		public async Task<string> GetFullTopic()
		{
			string Result = this.localTopic;

			IMqttTopicNode Loop = await this.GetParent() as IMqttTopicNode;
			while (!(Loop is null))
			{
				Result = Loop.LocalTopic + "/" + Result;
				Loop = await Loop.GetParent() as IMqttTopicNode;
			}

			return Result;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async Task<MqttBrokerNode> GetBroker()
		{
			INode Parent = await this.GetParent();

			while (!(Parent is null))
			{
				if (Parent is MqttBrokerNode Broker)
					return Broker;
				else if (Parent is MeteringNode MeteringNode)
					Parent = await MeteringNode.GetParent();
				else
					Parent = Parent.Parent;
			}

			return null;
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
			MqttBroker Broker = (await this.GetBroker())?.GetBroker();
			if (Broker is null)
				return null;
			else
				return await Broker.GetTopic(await this.GetFullTopic(), false, false);
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
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public override async Task<ControlParameter[]> GetControlParameters()
		{
			List<ControlParameter> Parameters = new List<ControlParameter>();
			Parameters.AddRange(await base.GetControlParameters());

			if (this.GetTopic()?.Result?.GetControlParameters() is ControlParameter[] P)
				Parameters.AddRange(P);

			return Parameters.ToArray();
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		/// <param name="DoneAfter">If readout is done after reporting fields (true), or if more fields will
		/// be reported by the caller (false).</param>
		public override async Task StartReadout(ISensorReadout Request, bool DoneAfter)
		{
			try
			{
				MqttTopic Topic = await this.GetTopic();

				await base.StartReadout(Request, (Topic is null) && DoneAfter);

				if (!(Topic is null))
					await Topic.StartReadout(Request, DoneAfter);
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

		/// <summary>
		/// Gets the default data object, if any.
		/// </summary>
		/// <returns>Default data object, if one exists, or null otherwise.</returns>
		public virtual Task<IMqttData> GetDefaultDataObject()
		{
			return Task.FromResult<IMqttData>(null);
		}

	}
}
