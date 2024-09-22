using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	/// TODO
	/// </summary>
	public class MqttTopicNode : ProvisionedMeteringNode, ISensor, IActuator
	{
		private string localTopic = string.Empty;

		/// <summary>
		/// TODO
		/// </summary>
		public MqttTopicNode()
			: base()
		{
		}

		/// <summary>
		/// TODO
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
		/// TODO
		/// </summary>
		public override string LocalId => this.localTopic;

		/// <summary>
		/// TODO
		/// </summary>
		[IgnoreMember]
		public string FullTopic
		{
			get
			{
				string Result = this.localTopic;

				MqttTopicNode Loop = this.Parent as MqttTopicNode;
				while (!(Loop is null))
				{
					Result = Loop.localTopic + "/" + Result;
					Loop = Loop.Parent as MqttTopicNode;
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
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is MqttTopicNode);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is MqttTopicNode || Parent is MqttBrokerNode);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 24, "Topic");
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
				return await Broker.GetTopic(this.FullTopic, false);
		}

		/// <summary>
		/// TODO
		/// </summary>
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
			if (Child is MqttTopicNode Topic)
				(await this.GetTopic())?.Remove(Topic.LocalTopic);

			return await base.RemoveAsync(Child);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult<ControlParameter[]>(this.GetTopic()?.Result?.GetControlParameters() ?? new ControlParameter[0]);
		}

		/// <summary>
		/// TODO
		/// </summary>
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
