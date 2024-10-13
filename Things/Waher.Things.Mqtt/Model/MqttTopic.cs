using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model
{
	/// <summary>
	/// MQTT Topic information.
	/// </summary>
	public class MqttTopic
	{
		private readonly SortedDictionary<string, MqttTopic> topics = new SortedDictionary<string, MqttTopic>();
		private readonly IMqttTopicNode node;
		private readonly ThingReference nodeReference;
		private readonly MqttTopic parent;
		private readonly MqttBroker broker;
		private readonly string localTopic;
		private readonly string fullTopic;
		private long dataCount = 0;
		private Exception ex = null;
		private DateTime exTP = DateTime.MinValue;
		private IMqttData data = null;

		/// <summary>
		/// MQTT Topic information.
		/// </summary>
		public MqttTopic(IMqttTopicNode Node, string FullTopic, string LocalTopic, MqttTopic Parent, MqttBroker Broker)
		{
			this.node = Node;
			this.fullTopic = FullTopic;
			this.localTopic = LocalTopic;
			this.parent = Parent;
			this.broker = Broker;

			this.nodeReference = Node as ThingReference;
			if (this.nodeReference is null && !(Node is null))
				this.nodeReference = new ThingReference(Node.NodeId, Node.SourceId, Node.Partition);
		}

		/// <summary>
		/// Reference to the MQTT Topic Node
		/// </summary>
		public IMqttTopicNode Node => this.node;

		/// <summary>
		/// MQTT Broker
		/// </summary>
		public MqttBroker Broker => this.broker;

		/// <summary>
		/// Local topic name.
		/// </summary>
		public string LocalTopic => this.localTopic;

		/// <summary>
		/// Full topic name
		/// </summary>
		public string FullTopic => this.fullTopic;

		/// <summary>
		/// Current parsed data.
		/// </summary>
		public IMqttData Data => this.data;

		private MqttTopic[] GetChildNodes()
		{
			if (this.topics is null)
				return new MqttTopic[0];
			else
			{
				MqttTopic[] Result;

				lock (this.topics)
				{
					Result = new MqttTopic[this.topics.Count];
					this.topics.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		internal async Task<MqttTopic> GetTopic(MqttTopicRepresentation Representation, bool CreateNew, bool IgnoreGuids, MqttBroker Broker)
		{
			MqttTopic Topic = await this.GetLocalTopic(Representation, CreateNew, IgnoreGuids, Broker);

			if (Topic is null)
				return null;
			else if (Representation.MoveNext(Topic))
				return await Topic.GetTopic(Representation, CreateNew, IgnoreGuids, Broker);
			else
				return Topic;
		}

		private async Task<MqttTopic> GetLocalTopic(MqttTopicRepresentation Representation, bool CreateNew, bool IgnoreGuids, MqttBroker Broker)
		{
			string CurrentSegment = Representation.CurrentSegment;
			MqttTopic Topic, Topic2;

			lock (this.topics)
			{
				if (this.topics.TryGetValue(CurrentSegment, out Topic))
					return Topic;
			}

			if (IgnoreGuids && Guid.TryParse(CurrentSegment.Replace('_', '-'), out Guid _))
				return null;

			if (this.node.HasChildren)
			{
				foreach (INode Child in await this.node.ChildNodes)
				{
					if (Child is IMqttTopicNode TopicNode && TopicNode.LocalTopic == CurrentSegment)
					{
						lock (this.topics)
						{
							if (this.topics.TryGetValue(CurrentSegment, out Topic2))
								return Topic2;
							else
							{
								Topic = new MqttTopic(TopicNode, Representation.ProcessedSegments, CurrentSegment, null, Broker);
								this.topics[CurrentSegment] = Topic;
								return Topic;
							}
						}
					}
				}
			}

			if (!CreateNew)
				return null;

			IMqttTopicNode AddNode = Types.FindBest<IMqttTopicNode, MqttTopicRepresentation>(Representation);
			if (AddNode is null)
				return null;

			AddNode = await AddNode.CreateNew(Representation);
			Topic = new MqttTopic(AddNode, Representation.ProcessedSegments, AddNode.LocalTopic, null, Broker);

			lock (this.topics)
			{
				if (this.topics.TryGetValue(CurrentSegment, out Topic2))
					return Topic2;
				else
					this.topics[CurrentSegment] = Topic;
			}

			await this.node.AddAsync(AddNode);

			return Topic;
		}

		/// <summary>
		/// Sets the parsed data of a topic.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="Data">Parsed data.</param>
		public void SetData<T>(T Data)
			where T : IMqttData
		{
			this.data = Data;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Content">Published MQTT Content</param>
		public async Task DataReported(MqttContent Content)
		{
			int Len = Content.Data.Length;
			if (Len == 0)
			{
				this.data = null;
				return;
			}

			this.dataCount += Len;

			try
			{
				if (!(this.data is null) && !await this.data.DataReported(this, Content))
					this.data = null;

				if (this.data is null)
					this.data = this.FindDataType(Content).CreateNew(this, Content);

				await this.SetOk();
			}
			catch (Exception)
			{
				this.data = this.FindDataType(Content).CreateNew(this, Content);
			}

			if (this.broker.Client?.HasSniffers ?? false)
				this.data.SnifferOutput(this.broker.Client);

			try
			{
				InternalReadoutRequest Request = new InternalReadoutRequest(string.Empty,
					new IThingReference[] { this.node }, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
					(sender, e) =>
					{
						this.node.NewMomentaryValues(e.Fields);

						MqttTopic Current = this;
						MqttTopic Parent = this.parent;

						while (!(Parent is null))
						{
							foreach (Field F in e.Fields)
							{
								if (F.Name == "Value")
									F.Name = Current.localTopic;
								else
									F.Name = Current.localTopic + ", " + F.Name;

								Parent.node.NewMomentaryValues(F);
							}

							Current = Parent;
							Parent = Parent.parent;
						}

						return Task.CompletedTask;
					},
					(sender, e) =>
					{
						return Task.CompletedTask;
					}, null);

				this.StartReadout(Request);
			}
			catch (Exception ex)
			{
				await this.Exception(ex);
			}
		}

		private IMqttData FindDataType(MqttContent Content)
		{
			try
			{
				IMqttData Data = Types.FindBest<IMqttData, MqttContent>(Content);
				if (!(Data is null))
					return Data;

				return new BinaryData(this, Content.Data);
			}
			catch (Exception)
			{
				return new BinaryData(this, Content.Data);
			}
		}

		private Task SetOk()
		{
			this.ex = null;
			this.exTP = DateTime.MinValue;

			return this.node.RemoveErrorAsync("Error");
		}

		private Task Exception(Exception ex)
		{
			this.ex = ex;
			this.exTP = DateTime.UtcNow;

			return this.node.LogErrorAsync("Error", ex.Message);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string ToString()
		{
			return this.fullTopic;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public void StartReadout(ISensorReadout Request)
		{
			Task.Run(() => this.StartReadout(this.nodeReference, Request, string.Empty, true));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async Task StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			try
			{
				MqttTopic[] ChildNodes = this.GetChildNodes();

				if (!(ChildNodes is null) && ChildNodes.Length > 0)
				{
					foreach (MqttTopic ChildTopic in ChildNodes)
					{
						await ChildTopic.StartReadout(ThingReference, Request,
							string.IsNullOrEmpty(Prefix) ? ChildTopic.LocalTopic : Prefix + ", " + ChildTopic.LocalTopic, false);
					}
				}

				if (!(this.ex is null))
					Request.ReportErrors(Last, new ThingError(ThingReference, this.exTP, this.ex.Message));
				else if (this.data is null)
				{
					if (Last)
						Request.ReportFields(true);
				}
				else
					await this.data.StartReadout(ThingReference, Request, Prefix, Last);

				await this.node.RemoveErrorAsync("Readout");
			}
			catch (Exception ex)
			{
				Request.ReportErrors(Last, new ThingError(ThingReference, DateTime.UtcNow, ex.Message));
				await this.node.LogErrorAsync("Readout", ex.Message);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public ControlParameter[] GetControlParameters()
		{
			if (this.data is null || !this.data.IsControllable)
				return new ControlParameter[0];
			else
				return this.data.GetControlParameters();
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(LinkedList<Parameter> Parameters,
			Language Language, RequestOrigin _)
		{
			if (!(this.data is null))
			{
				Parameters.AddLast(new StringParameter("Type", await Language.GetStringAsync(typeof(MqttTopicNode), 25, "Type"),
					await this.data.GetTypeName(Language)));
			}

			if (this.dataCount > 0)
			{
				Parameters.AddLast(new Int64Parameter("Data Count", await Language.GetStringAsync(typeof(MqttTopicNode), 26, "Data Count"),
					this.dataCount));
			}

			return Parameters;
		}

		/// <summary>
		/// Removes a child topic
		/// </summary>
		/// <param name="LocalTopic">Local topic name.</param>
		/// <returns>If local topic was found and removed.</returns>
		public bool Remove(string LocalTopic)
		{
			if (!(LocalTopic is null))
			{
				lock (this.topics)
				{
					return this.topics.Remove(LocalTopic);
				}
			}
			else
				return false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public MqttClient MqttClient => this.broker?.Client;
	}
}
