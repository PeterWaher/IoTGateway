using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.MQTT;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Timing;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Metering;
using Waher.Things.Mqtt.Model;
using System.Threading;

namespace Waher.Things.Mqtt
{
	public class MqttBrokerNode : IpHostPort, ISniffable
	{
		private MqttQualityOfService willQoS = MqttQualityOfService.AtLeastOnce;
		private string userName = string.Empty;
		private string password = string.Empty;
		private string willTopic = string.Empty;
		private string willData = string.Empty;
		private string brokerKey = null;
		private bool tls = true;
		private bool willRetain = false;

		public MqttBrokerNode()
			: base()
		{
			this.Port = 8883;
		}

		[Page(7, "IP")]
		[Header(8, "Encrypted (TLS)", 50)]
		[ToolTip(9, "Check if Transpotrt Layer Encryption (TLS) should be used.")]
		[DefaultValue(true)]
		public bool Tls
		{
			get { return this.tls; }
			set { this.tls = value; }
		}

		[Page(2, "MQTT")]
		[Header(3, "User Name:")]
		[ToolTip(4, "User name used during authentication process.")]
		[DefaultValueStringEmpty]
		public string UserName
		{
			get => this.userName;
			set => this.userName = value;
		}

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

		[Page(10, "Last Will and Testament")]
		[Header(11, "Will Topic:")]
		[ToolTip(12, "When the connection is lost, a Last Will and Testament can be published on this topic to alert subscribers you've lost connection.")]
		[DefaultValueStringEmpty]
		public string WillTopic
		{
			get => this.willTopic;
			set => this.willTopic = value;
		}

		[Page(10, "Last Will and Testament")]
		[Header(13, "Will Data:")]
		[ToolTip(14, "When the connection is lost, this content will be published on the topic defined above.")]
		[DefaultValueStringEmpty]
		public string WillData
		{
			get => this.willData;
			set => this.willData = value;
		}

		[Page(10, "Last Will and Testament")]
		[Header(15, "Retain Will on topic.")]
		[ToolTip(16, "If the content published on the will should be retained on the topic.")]
		[DefaultValue(false)]
		public bool WillRetain
		{
			get { return this.willRetain; }
			set { this.willRetain = value; }
		}

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

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttBrokerNode), 1, "MQTT Broker");
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is MqttTopicNode);
		}

		public override Task DestroyAsync()
		{
			if (!string.IsNullOrEmpty(this.brokerKey))
				MqttBrokers.DestroyBroker(this.brokerKey);

			return base.DestroyAsync();
		}

		[IgnoreMember]
		public string Key
		{
			get
			{
				string PrevKey = this.brokerKey;
				this.brokerKey = MqttBrokers.GetKey(this.Host, this.Port, this.tls, this.userName, this.password);

				if (PrevKey != this.brokerKey && !string.IsNullOrEmpty(PrevKey))
					MqttBrokers.DestroyBroker(PrevKey);

				return this.brokerKey;
			}
		}

		protected override Task NodeUpdated()
		{
			this.GetBroker();

			return base.NodeUpdated();
		}

		internal MqttBroker GetBroker()
		{
			return MqttBrokers.GetBroker(this, this.Key, this.Host, this.Port, this.tls, this.userName, this.password,
				this.willTopic, this.willData, this.willRetain, this.willQoS);
		}

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
		/// <see cref="ISniffable.GetEnumerator"/>
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

		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		public async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();

			Result.AddRange(await base.Commands);
			Result.Add(new ReconnectCommand(this.GetBroker().Client));

			return Result;
		}

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
