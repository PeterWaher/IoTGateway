using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Things.Metering;

namespace Waher.Things.Mqtt.Model
{
	public class MqttBroker : IDisposable
	{
		private static Scheduler scheduler = null;
		private readonly SortedDictionary<string, MqttTopic> topics = new SortedDictionary<string, MqttTopic>();
		private readonly MqttBrokerNode node;
		private MqttClient mqttClient;
		private bool connectionOk = false;
		private readonly string host;
		private readonly int port;
		private readonly bool tls;
		private readonly string userName;
		private readonly string password;
		private string willTopic;
		private string willData;
		private bool willRetain;
		private MqttQualityOfService willQoS;
		private DateTime nextCheck;

		public MqttBroker(MqttBrokerNode Node, string Host, int Port, bool Tls, string UserName, string Password,
			string WillTopic, string WillData, bool WillRetain, MqttQualityOfService WillQoS)
		{
			this.node = Node;
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.userName = UserName;
			this.password = Password;
			this.willTopic = WillTopic;
			this.willData = WillData;
			this.willRetain = WillRetain;
			this.willQoS = WillQoS;

			this.Open();
		}

		internal MqttClient Client => this.mqttClient;

		private void Open()
		{
			this.mqttClient = new MqttClient(this.host, this.port, this.tls, this.userName, this.password, this.willTopic,
				this.willQoS, this.willRetain, Encoding.UTF8.GetBytes(this.willData));

			this.mqttClient.OnConnectionError += MqttClient_OnConnectionError;
			this.mqttClient.OnContentReceived += MqttClient_OnContentReceived;
			this.mqttClient.OnStateChanged += MqttClient_OnStateChanged;

			this.nextCheck = Scheduler.Add(DateTime.Now.AddMinutes(1), this.CheckOnline, null);
		}

		private void Close()
		{
			if (this.mqttClient != null)
			{
				Scheduler.Remove(this.nextCheck);

				this.mqttClient.OnConnectionError -= MqttClient_OnConnectionError;
				this.mqttClient.OnContentReceived -= MqttClient_OnContentReceived;
				this.mqttClient.OnStateChanged -= MqttClient_OnStateChanged;

				this.mqttClient.Dispose();
				this.mqttClient = null;
			}
		}

		public void Dispose()
		{
			this.Close();
		}

		private void CheckOnline(object _)
		{
			if (this.mqttClient != null)
			{
				try
				{
					MqttState State = this.mqttClient.State;
					if (State == MqttState.Offline || State == MqttState.Error)
						this.mqttClient.Reconnect();
				}
				finally
				{
					this.nextCheck = Scheduler.Add(DateTime.Now.AddMinutes(1), this.CheckOnline, null);
				}
			}
		}

		public async Task DataReceived(MqttContent Content)
		{
			MqttTopic Topic = await this.GetTopic(Content.Topic, true);
			Topic?.DataReported(Content);
		}

		public void SetWill(string WillTopic, string WillData, bool WillRetain, MqttQualityOfService WillQoS)
		{
			if (this.willTopic != WillTopic || this.willData != WillData || this.willRetain != WillRetain || this.willQoS != WillQoS)
			{
				this.Close();

				this.willTopic = WillTopic;
				this.willData = WillData;
				this.willRetain = WillRetain;
				this.willQoS = WillQoS;

				this.Open();
			}
		}

		private async void MqttClient_OnStateChanged(object Sender, MqttState NewState)
		{
			try
			{
				switch (NewState)
				{
					case MqttState.Connected:
						this.connectionOk = true;
						await this.node.RemoveErrorAsync("Offline");
						await this.node.RemoveErrorAsync("Error");

						this.mqttClient.SUBSCRIBE("#", MqttQualityOfService.AtLeastOnce);
						break;

					case MqttState.Error:
						await this.node.LogErrorAsync("Error", "Connection to broker failed.");
						this.Reconnect();
						break;

					case MqttState.Offline:
						await this.node.LogErrorAsync("Offline", "Connection is closed.");
						this.Reconnect();
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void MqttClient_OnContentReceived(object Sender, MqttContent Content)
		{
			lock (this.queue)
			{
				if (this.processing)
				{
					this.queue.AddLast(Content);
					return;
				}
				else
					this.processing = true;
			}

			this.Process(Content);
		}

		private readonly LinkedList<MqttContent> queue = new LinkedList<MqttContent>();
		private bool processing = false;

		private async void Process(MqttContent Content)
		{
			try
			{
				while (true)
				{
					MqttTopic Topic = await this.GetTopic(Content.Topic, true);
					Topic?.DataReported(Content);

					lock (this.queue)
					{
						if (this.queue.First == null)
						{
							this.processing = false;
							break;
						}
						else
						{
							Content = this.queue.First.Value;
							this.queue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.processing = false;
			}
		}

		private void MqttClient_OnConnectionError(object Sender, Exception Exception)
		{
			this.Reconnect();
		}

		private void Reconnect()
		{
			if (this.connectionOk)
			{
				this.connectionOk = false;
				this.mqttClient?.Reconnect();
			}
		}

		public async Task<MqttTopic> GetTopic(string TopicString, bool CreateNew)
		{
			if (string.IsNullOrEmpty(TopicString))
				return null;

			string[] Parts = TopicString.Split('/');
			MqttTopic Topic;

			lock (this.topics)
			{
				if (!this.topics.TryGetValue(Parts[0], out Topic))
					Topic = null;
			}

			if (Topic == null)
			{
				if (System.Guid.TryParse(Parts[0].Replace('_', '-'), out Guid _))
					return null;

				if (this.node.HasChildren)
				{
					foreach (INode Child in await this.node.ChildNodes)
					{
						if (Child is MqttTopicNode Topic2 && Topic2.LocalTopic == Parts[0])
						{
							Topic = new MqttTopic(Topic2, Parts[0], Parts[0], null, this);
							break;
						}
					}
				}

				MqttTopicNode Node = null;

				if (Topic == null)
				{
					if (!CreateNew)
						return null;

					Node = new MqttTopicNode()
					{
						NodeId = await MeteringNode.GetUniqueNodeId(Parts[0]),
						LocalTopic = Parts[0]
					};

					Topic = new MqttTopic(Node, Parts[0], Parts[0], null, this);
				}

				lock (this.topics)
				{
					if (this.topics.ContainsKey(Parts[0]))
						Topic = this.topics[Parts[0]];
					else
						this.topics[Parts[0]] = Topic;
				}

				if (Node != null)
				{
					if (Node != Topic.Node)
						await Node.DestroyAsync();
					else
						await this.node.AddAsync(Node);
				}
			}

			if (Parts.Length == 1)
				return Topic;
			else
				return await Topic.GetTopic(Parts, 1, CreateNew, this);
		}

		public bool Remove(string LocalTopic)
		{
			if (LocalTopic != null)
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
		/// Scheduler for asynchronous tasks.
		/// </summary>
		public static Scheduler Scheduler
		{
			get
			{
				if (scheduler is null)
				{
					if (Types.TryGetModuleParameter("Scheduler", out object Obj) && Obj is Scheduler Scheduler)
						scheduler = Scheduler;
					else
					{
						scheduler = new Scheduler();

						Log.Terminating += (sender, e) =>
						{
							scheduler?.Dispose();
							scheduler = null;
						};
					}
				}

				return scheduler;
			}
		}

	}
}
