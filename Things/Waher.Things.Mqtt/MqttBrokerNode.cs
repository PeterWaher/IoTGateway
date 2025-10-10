using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Mqtt.Model;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// Node representing a connection to an MQTT broker.
	/// </summary>
	public class MqttBrokerNode : IpHostPort, ICommunicationLayer, IEncryptedProperties
	{
		private MqttQualityOfService willQoS = MqttQualityOfService.AtLeastOnce;
		private string userName = string.Empty;
		private string password = string.Empty;
		private string willTopic = string.Empty;
		private string willData = string.Empty;
		private string brokerKey = null;
		private string connectionSubscription = "#";
		private bool willRetain = false;
		private bool trustServer = false;

		/// <summary>
		/// Node representing a connection to an MQTT broker.
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
		[Encrypted(32)]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[] { nameof(this.Password) };

		/// <summary>
		/// Startup subscription
		/// </summary>
		[Page(2, "MQTT")]
		[Header(46, "Connection Subscription:")]
		[ToolTip(47, "Subscription topic executed when connecting. Empty means no subscription will be performed. Multiple subjects can be comma-separated.")]
		[DefaultValue("#")]
		public string ConnectionSubscription
		{
			get => this.connectionSubscription;
			set => this.connectionSubscription = value;
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
		/// Type name representing data.
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
				this.brokerKey = MqttBrokers.GetKey(this.Host, this.Port, this.Tls, this.trustServer, this.userName, this.password,
					this.connectionSubscription);

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

		/// <summary>
		/// Gets the corresponding broker node.
		/// </summary>
		/// <returns>MQTT Broker connection object.</returns>
		public Task<MqttBroker> GetBroker()
		{
			return MqttBrokers.GetBroker(this, this.Key, this.Host, this.Port, this.Tls, this.TrustServer, this.userName, this.password,
				this.connectionSubscription, this.willTopic, this.willData, this.willRetain, this.willQoS);
		}

		/// <summary>
		/// Gets the corresponding broker node, if available in the cache.
		/// </summary>
		/// <returns>MQTT Broker connection object, or null if none in the cache.</returns>
		public MqttBroker GetCachedBroker()
		{
			return MqttBrokers.GetCachedBroker(this.Key);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<bool> RemoveAsync(INode Child)
		{
			if (Child is MqttTopicNode Topic)
				(await this.GetBroker()).Remove(Topic.LocalTopic);

			return await base.RemoveAsync(Child);
		}

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => true;

		/// <summary>
		/// <see cref="ICommunicationLayer.Add"/>
		/// </summary>
		public void Add(ISniffer Sniffer)
		{
			this.GetBroker().Result.Client?.Add(Sniffer);   // TODO: Avoid blocking call
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.AddRange"/>
		/// </summary>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.GetBroker().Result.Client?.AddRange(Sniffers); // TODO: Avoid blocking call
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.Remove"/>
		/// </summary>
		public bool Remove(ISniffer Sniffer)
		{
			return this.GetBroker().Result.Client?.Remove(Sniffer) ?? false;    // TODO: Avoid blocking call
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get { return this.GetBroker().Result.Client?.Sniffers ?? Array.Empty<ISniffer>(); } // TODO: Avoid blocking call
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers
		{
			get { return this.GetBroker().Result.Client?.HasSniffers ?? false; }    // TODO: Avoid blocking call
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
			return this.GetBroker().Result.Client?.GetEnumerator() ?? Array.Empty<ISniffer>().GetEnumerator();  // TODO: Avoid blocking call
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveBinary(Count);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data)
		{
			this.ReceiveBinary(ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveBinary(ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitBinary(Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data)
		{
			this.TransmitBinary(ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitBinary(ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveText(Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitText(Text);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Information(Comment);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Warning(Warning);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Error(Error);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Exception(Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Exception(Exception);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveBinary(Timestamp, Count);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			this.ReceiveBinary(Timestamp, ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitBinary(Timestamp, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			this.TransmitBinary(Timestamp, ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.ReceiveText(Timestamp, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.TransmitText(Timestamp, Text);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Information(Timestamp, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Warning(Timestamp, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Error(Timestamp, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Exception(Timestamp, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception)
		{
			MqttBroker Broker = this.GetCachedBroker();
			MqttClient Client = Broker?.Client;
			Client?.Exception(Timestamp, Exception);
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
			Result.Add(new ReconnectCommand((await this.GetBroker()).Client));

			return Result;
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;
			MqttBroker Broker = await this.GetBroker();

			Result.AddLast(new StringParameter("State", await Language.GetStringAsync(typeof(MqttBrokerNode), 30, "State"),
				Broker.Client.State.ToString() ?? string.Empty));

			return Result;
		}

	}
}
