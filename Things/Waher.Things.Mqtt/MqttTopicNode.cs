using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Persistence.Attributes;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Mqtt
{
	public class MqttTopicNode : ProvisionedMeteringNode, ISensor, IActuator
	{
		private string localTopic = string.Empty;

		public MqttTopicNode()
			: base()
		{
		}

		[Page(2, "MQTT")]
		[Header(22, "Local Topic:")]
		[ToolTip(23, "Local topic, unique among siblings.")]
		[DefaultValueStringEmpty]
		public string LocalTopic
		{
			get => this.localTopic;
			set => this.localTopic = value;
		}

		public override string LocalId => this.localTopic;

		[IgnoreMember]
		public string FullTopic
		{
			get
			{
				string Result = this.localTopic;

				MqttTopicNode Loop = this.Parent as MqttTopicNode;
				while (Loop != null)
				{
					Result = Loop.localTopic + "/" + Result;
					Loop = Loop.Parent as MqttTopicNode;
				}

				return Result;
			}
		}

		[IgnoreMember]
		public MqttBrokerNode Broker
		{
			get
			{
				INode Parent = this.Parent as INode;

				while (Parent != null)
				{
					if (Parent is MqttBrokerNode Broker)
						return Broker;

					Parent = Parent.Parent as INode;
				}

				return null;
			}
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is MqttTopicNode);
		}

		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is MqttTopicNode || Parent is MqttBrokerNode);
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 24, "Topic");
		}

		public async Task<MqttTopic> GetTopic()
		{
			MqttBroker Broker = this.Broker?.GetBroker();
			return await Broker?.GetTopic(this.FullTopic, false);
		}

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Parameters = (LinkedList<Parameter>)await base.GetDisplayableParametersAsync(Language, Caller);
			MqttTopic Topic = await this.GetTopic();

			if (Topic != null)
				await Topic.GetDisplayableParametersAsync(Parameters, Language, Caller);

			return Parameters;
		}

		public async override Task<bool> RemoveAsync(INode Child)
		{
			if (Child is MqttTopicNode Topic)
				(await this.GetTopic())?.Remove(Topic.LocalTopic);

			return await base.RemoveAsync(Child);
		}

		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult<ControlParameter[]>(this.GetTopic()?.Result?.GetControlParameters() ?? new ControlParameter[0]);
		}

		public async Task StartReadout(ISensorReadout Request)
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
	}
}
