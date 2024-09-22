using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model.Encapsulations;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model
{
	/// <summary>
	/// TODO
	/// </summary>
	public class MqttTopic
	{
		private readonly SortedDictionary<string, MqttTopic> topics = new SortedDictionary<string, MqttTopic>();
		private readonly SemaphoreSlim topicSemaphore = new SemaphoreSlim(1);
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
		/// TODO
		/// </summary>
		public MqttTopic(IMqttTopicNode Node, string FullTopic, string LocalTopic, MqttTopic Parent, MqttBroker Broker)
		{
			this.node = Node;
			this.fullTopic = FullTopic;
			this.localTopic = LocalTopic;
			this.parent = Parent;
			this.broker = Broker;

			this.nodeReference = Node as ThingReference;
			if (this.nodeReference is null)
				this.nodeReference = new ThingReference(Node.NodeId, Node.SourceId, Node.Partition);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public IMqttTopicNode Node => this.node;

		/// <summary>
		/// TODO
		/// </summary>
		public string LocalTopic => this.localTopic;

		/// <summary>
		/// TODO
		/// </summary>
		public string FullTopic => this.fullTopic;

		private async Task<MqttTopic[]> GetChildNodes()
		{
			if (this.topics is null)
				return new MqttTopic[0];
			else
			{
				MqttTopic[] Result;

				await (this.topicSemaphore.WaitAsync());
				try
				{
					Result = new MqttTopic[this.topics.Count];
					this.topics.Values.CopyTo(Result, 0);
				}
				finally
				{
					this.topicSemaphore.Release();
				}

				return Result;
			}
		}

		internal async Task<MqttTopic> GetTopic(MqttTopicRepresentation Representation, bool CreateNew, MqttBroker Broker)
		{
			string CurrentSegment = Representation.CurrentSegment;
			MqttTopic Topic;

			await this.topicSemaphore.WaitAsync();
			try
			{
				if (!this.topics.TryGetValue(CurrentSegment, out Topic))
					Topic = null;

				if (Topic is null)
				{
					if (Guid.TryParse(CurrentSegment.Replace('_', '-'), out Guid _))
						return null;

					if (this.node.HasChildren)
					{
						foreach (INode Child in await this.node.ChildNodes)
						{
							if (Child is IMqttTopicNode Topic2 && Topic2.LocalTopic == CurrentSegment)
							{
								Topic = new MqttTopic(Topic2, Topic2.FullTopic, CurrentSegment, null, Broker);
								break;
							}
						}
					}

					if (Topic is null)
					{
						if (!CreateNew)
							return null;

						IMqttTopicNode Node = Types.FindBest<IMqttTopicNode, MqttTopicRepresentation>(Representation);
						if (Node is null)
							return null;

						Node = await Node.CreateNew(Representation);
						Topic = new MqttTopic(Node, Representation.ProcessedSegments, Node.LocalTopic, null, Broker);

						await this.node.AddAsync(Node);
					}

					this.topics[Topic.LocalTopic] = Topic;
				}
			}
			finally
			{
				this.topicSemaphore.Release();
			}

			if (Representation.MoveNext())
				return await Topic.GetTopic(Representation, CreateNew, Broker);
			else
				return Topic;
		}

		/// <summary>
		/// TODO
		/// </summary>
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
				if (!(this.data?.DataReported(Content) ?? false))
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

			return this.node.RemoveErrorAsync("Eror");
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
				MqttTopic[] ChildNodes = await this.GetChildNodes();

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
					this.data.StartReadout(ThingReference, Request, Prefix, Last);

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
		public async Task<bool> Remove(string LocalTopic)
		{
			if (!(LocalTopic is null))
			{
				await this.topicSemaphore.WaitAsync();
				try
				{
					return this.topics.Remove(LocalTopic);
				}
				finally
				{
					this.topicSemaphore.Release();
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
