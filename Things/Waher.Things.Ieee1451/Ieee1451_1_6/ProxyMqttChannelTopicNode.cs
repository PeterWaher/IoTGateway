using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds;
using Waher.Things.Metering;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;
using Waher.Things.SensorData;

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
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		/// <param name="DoneAfter">If readout is done after reporting fields (true), or if more fields will
		/// be reported by the caller (false).</param>
		public override async Task StartReadout(ISensorReadout Request, bool DoneAfter)
		{
			try
			{
				await base.StartReadout(Request, false);

				Field[] Fields = await this.ReadSensor(Request.Actor);
				Request.ReportFields(DoneAfter, Fields);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(DoneAfter, new ThingError(this, ex.Message));
			}
		}

		/// <summary>
		/// Reads the transducer value.
		/// </summary>
		/// <returns>Fields read</returns>
		/// <exception cref="Exception">If proxy node is not found, is not a sensor, or cannot be read.</exception>
		public async Task<Field[]> ReadSensor(string Actor)
		{
			MeteringNode Node = await MeteringTopology.GetNode(this.proxyNodeId)
				?? throw new Exception("Node not found in Metering Topology: " + this.proxyNodeId);

			if (!(Node is ISensor Sensor))
				throw new Exception("Node not a sensor node: " + this.proxyNodeId);

			List<Field> Fields = new List<Field>();
			List<ThingError> Errors = new List<ThingError>();
			TaskCompletionSource<bool> Completed = new TaskCompletionSource<bool>();
			InternalReadoutRequest Readout = new InternalReadoutRequest(Actor,
				new IThingReference[] { Sensor }, FieldType.Momentary,
				new string[] { this.proxyFieldName }, DateTime.MinValue, DateTime.MaxValue,
				(_, e) =>
				{
					Fields.AddRange(e.Fields);
					if (e.Done)
						Completed.TrySetResult(true);

					return Task.CompletedTask;
				},
				(_, e) =>
				{
					Errors.AddRange(e.Errors);
					if (e.Done)
						Completed.TrySetResult(true);

					return Task.CompletedTask;
				}, null);

			_ = Task.Delay(this.TimeoutMilliseconds).ContinueWith(
				(_) => Completed.TrySetException(new TimeoutException()));

			await Completed.Task;

			if (Errors.Count > 0)
			{
				foreach (ThingError Error in Errors)
					throw new Exception(Error.ErrorMessage);
			}

			return Fields.ToArray();
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
		public async Task TedsRequest(TedsAccessMessage TedsAccessMessage,
			TedsAccessCode TedsAccessCode, uint TedsOffset, double TimeoutSeconds)
		{
			if (!(await this.GetParent() is ProxyMqttTimTopicNode TimNode))
				return;

			if (!(await TimNode.GetParent() is ProxyMqttNcapTopicNode NcapNode))
				return;

			if (!(await NcapNode.GetParent() is DiscoverableTopicNode CommunicationNode))
				return;

			MqttBrokerNode BrokerNode = await CommunicationNode.GetBroker();
			if (BrokerNode is null)
				return;

			MqttBroker Broker = BrokerNode.GetBroker();
			if (Broker is null)
				return;

			string Topic = await CommunicationNode.GetFullTopic();
			if (string.IsNullOrEmpty(Topic))
				return;

			byte[] Response;

			switch (TedsAccessCode)
			{
				case TedsAccessCode.XdcrName:
					Response = TedsAccessMessage.SerializeResponse(0, this.NcapIdBinary,
						this.TimIdBinary, (ushort)this.ChannelId, new TedsId(),
						new Format(true),
						new Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds.Content(this.EntityName));
					break;

				case TedsAccessCode.ChanTEDS:
				case TedsAccessCode.MetaTEDS:
				default:
					return;
			}

			await Broker.Publish(Topic, MqttQualityOfService.AtLeastOnce, false, Response);
		}

	}
}
