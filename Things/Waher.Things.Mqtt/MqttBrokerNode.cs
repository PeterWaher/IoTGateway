using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Networking.MQTT;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// TODO
	/// </summary>
	public class MqttBrokerNode : IpHostPort, ISniffable
	{
		private MqttQualityOfService willQoS = MqttQualityOfService.AtLeastOnce;
		private string userName = string.Empty;
		private string password = string.Empty;
		private string willTopic = string.Empty;
		private string willData = string.Empty;
		private string brokerKey = null;
		private bool willRetain = false;
		private bool trustServer = false;

		/// <summary>
		/// TODO
		/// </summary>
		public MqttBrokerNode()
			: base()
		{
			this.Port = 8883;
			this.Tls = true;
		}

		/// <summary>
		/// If connection is encrypted using TLS or not.
		/// </summary>
		[Page(1, "IP")]
		[Header(44, "Trust Server", 80)]
		[ToolTip(45, "If the remote server certificate should be trusted even if it is not valid.")]
		public bool TrustServer
		{
			get => this.trustServer;
			set => this.trustServer = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(2, "MQTT")]
		[Header(3, "User Name:")]
		[ToolTip(4, "User name used during authentication process.")]
		[DefaultValueStringEmpty]
		public string UserName
		{
			get => this.userName;
			set => this.userName = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(2, "MQTT")]
		[Header(5, "Password:")]
		[ToolTip(6, "Password used during authentication process. NOTE: Will be sent in clear text. Don't reuse passwords.")]
		[Masked]
		[DefaultValueStringEmpty]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(10, "Last Will and Testament")]
		[Header(11, "Will Topic:")]
		[ToolTip(12, "When the connection is lost, a Last Will and Testament can be published on this topic to alert subscribers you've lost connection.")]
		[DefaultValueStringEmpty]
		public string WillTopic
		{
			get => this.willTopic;
			set => this.willTopic = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(10, "Last Will and Testament")]
		[Header(13, "Will Data:")]
		[ToolTip(14, "When the connection is lost, this content will be published on the topic defined above.")]
		[DefaultValueStringEmpty]
		public string WillData
		{
			get => this.willData;
			set => this.willData = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(10, "Last Will and Testament")]
		[Header(15, "Retain Will on topic.")]
		[ToolTip(16, "If the content published on the will should be retained on the topic.")]
		[DefaultValue(false)]
		public bool WillRetain
		{
			get => this.willRetain;
			set => this.willRetain = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(10, "Last Will and Testament")]
		[Header(17, "Quality of Service:")]
		[ToolTip(18, "The quality of service used when sending the last will and testament.")]
		[DefaultValue(MqttQualityOfService.AtLeastOnce)]
		[Option(MqttQualityOfService.AtMostOnce, 19, "At most once")]
		[Option(MqttQualityOfService.AtLeastOnce, 20, "At least once")]
		[Option(MqttQualityOfService.ExactlyOnce, 19, "Exactly once")]
		public MqttQualityOfService WillQoS
		{
			get => this.willQoS;
			set => this.willQoS = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttBrokerNode), 1, "MQTT Broker");
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
		public override Task DestroyAsync()
		{
			if (!string.IsNullOrEmpty(this.brokerKey))
				MqttBrokers.DestroyBroker(this.brokerKey);

			return base.DestroyAsync();
		}

		/// <summary>
		/// TODO
		/// </summary>
		[IgnoreMember]
		public string Key
		{
			get
			{
				string PrevKey = this.brokerKey;
				this.brokerKey = MqttBrokers.GetKey(this.Host, this.Port, this.Tls, this.trustServer, this.userName, this.password);

				if (PrevKey != this.brokerKey && !string.IsNullOrEmpty(PrevKey))
					MqttBrokers.DestroyBroker(PrevKey);

				return this.brokerKey;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		protected override Task NodeUpdated()
		{
			this.GetBroker();

			return base.NodeUpdated();
		}

		internal MqttBroker GetBroker()
		{
			return MqttBrokers.GetBroker(this, this.Key, this.Host, this.Port, this.Tls, this.TrustServer, this.userName, this.password,
				this.willTopic, this.willData, this.willRetain, this.willQoS);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> RemoveAsync(INode Child)
		{
			if (Child is MqttTopicNode Topic)
				this.GetBroker().Remove(Topic.LocalTopic);

			return base.RemoveAsync(Child);
		}

		#region ISniffable

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public void Add(ISniffer Sniffer)
		{
			this.GetBroker().Client?.Add(Sniffer);
		}

		/// <summary>
		/// <see cref="ISniffable.AddRange"/>
		/// </summary>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.GetBroker().Client?.AddRange(Sniffers);
		}

		/// <summary>
		/// <see cref="ISniffable.Remove"/>
		/// </summary>
		public bool Remove(ISniffer Sniffer)
		{
			return this.GetBroker().Client?.Remove(Sniffer) ?? false;
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get { return this.GetBroker().Client?.Sniffers ?? new ISniffer[0]; }
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers
		{
			get { return this.GetBroker().Client?.HasSniffers ?? false; }
		}

		/// <summary>
		/// <see cref="IEnumerable{T}.GetEnumerator()"/>
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			return new SnifferEnumerator(this.Sniffers);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetBroker().Client?.GetEnumerator() ?? new ISniffer[0].GetEnumerator();
		}

		#endregion

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		/// <summary>
		/// TODO
		/// </summary>
		public async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();

			Result.AddRange(await base.Commands);
			Result.Add(new ReconnectCommand(this.GetBroker().Client));

			return Result;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;
			MqttBroker Broker = this.GetBroker();

			Result.AddLast(new StringParameter("State", await Language.GetStringAsync(typeof(MqttBrokerNode), 30, "State"),
				Broker.Client.State.ToString() ?? string.Empty));

			return Result;
		}

	}
}
