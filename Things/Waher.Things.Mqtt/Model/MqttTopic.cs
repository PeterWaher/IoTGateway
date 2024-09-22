using System;
using System.Collections.Generic;
using System.Text;
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
		private readonly MqttTopicNode node;
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
		public MqttTopic(MqttTopicNode Node, string FullTopic, string LocalTopic, MqttTopic Parent, MqttBroker Broker)
		{
			this.node = Node;
			this.fullTopic = FullTopic;
			this.localTopic = LocalTopic;
			this.parent = Parent;
			this.broker = Broker;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public MqttTopicNode Node => this.node;

		/// <summary>
		/// TODO
		/// </summary>
		public string LocalTopic => this.localTopic;

		/// <summary>
		/// TODO
		/// </summary>
		public string FullTopic => this.fullTopic;

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

		internal async Task<MqttTopic> GetTopic(string[] Parts, int Index, bool CreateNew, MqttBroker Broker)
		{
			MqttTopic Topic;

			lock (this.topics)
			{
				if (!this.topics.TryGetValue(Parts[Index], out Topic))
					Topic = null;
			}

			if (Topic is null)
			{
				if (Guid.TryParse(Parts[Index].Replace('_', '-'), out Guid _))
					return null;

				if (this.node.HasChildren)
				{
					foreach (INode Child in await this.node.ChildNodes)
					{
						if (Child is MqttTopicNode Topic2 && Topic2.LocalTopic == Parts[Index])
						{
							Topic = new MqttTopic(Topic2, Topic2.FullTopic, Parts[Index], null, Broker);
							break;
						}
					}
				}

				MqttTopicNode Node = null;

				if (Topic is null)
				{
					if (!CreateNew)
						return null;

					StringBuilder sb = new StringBuilder();
					int i;

					for (i = 0; i < Index; i++)
					{
						sb.Append(Parts[i]);
						sb.Append('/');
					}

					sb.Append(Parts[Index]);

					string FullTopic = sb.ToString();

					Node = new MqttTopicNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(FullTopic),
						LocalTopic = Parts[Index]
					};

					Topic = new MqttTopic(Node, FullTopic, Parts[Index], this, Broker);
				}

				lock (this.topics)
				{
					if (this.topics.ContainsKey(Parts[Index]))
						Topic = this.topics[Parts[Index]];
					else
						this.topics[Parts[Index]] = Topic;
				}

				if (!(Node is null))
				{
					if (Node != Topic.Node)
						await Node.DestroyAsync();
					else
						await this.node.AddAsync(Node);
				}
			}

			Index++;
			if (Parts.Length == Index)
				return Topic;
			else
				return await Topic.GetTopic(Parts, Index, CreateNew, Broker);
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
					this.data = this.FindDataType(Content);

				await this.SetOk();
			}
			catch (Exception)
			{
				this.data = this.FindDataType(Content);
			}

			if (this.broker.Client?.HasSniffers ?? false)
				this.data.SnifferOutput(this.broker.Client);

			try
			{
				InternalReadoutRequest Request = new InternalReadoutRequest(string.Empty,
					new ThingReference[] { this.node }, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
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
			Task.Run(() => this.StartReadout(this.node, Request, string.Empty, true));
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
		/// TODO
		/// </summary>
		public bool Remove(string LocalTopic)
		{
			lock (this.topics)
			{
				return this.topics.Remove(LocalTopic);
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public MqttClient MqttClient => this.broker?.Client;
	}
}
