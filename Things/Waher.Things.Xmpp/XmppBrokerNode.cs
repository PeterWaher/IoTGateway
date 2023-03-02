using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Xmpp.Model;

namespace Waher.Things.Xmpp
{
	public class XmppBrokerNode : IpHostPort, ISniffable
	{
		private string userName = string.Empty;
		private string password = string.Empty;
		private string passwordMechanism = string.Empty;
		private string brokerKey = null;
		private bool trustServer = false;
		private bool allowInsecureMechanisms = false;

		public XmppBrokerNode()
			: base()
		{
			this.Port = 5222;
			this.Tls = true;
		}

		[Page(2, "XMPP")]
		[Header(13, "User Name:")]
		[ToolTip(14, "User name used during authentication process.")]
		[DefaultValueStringEmpty]
		public string UserName
		{
			get => this.userName;
			set => this.userName = value;
		}

		[Page(2, "XMPP")]
		[Header(15, "Password:")]
		[ToolTip(16, "Password used during authentication process.")]
		[Masked]
		[DefaultValueStringEmpty]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}

		[Page(2, "XMPP")]
		[Header(17, "Password Hash Mechanism:")]
		[ToolTip(18, "Mechanism used for password hash.")]
		[DefaultValueStringEmpty]
		public string PasswordMechanism
		{
			get => this.passwordMechanism;
			set => this.passwordMechanism = value;
		}

		[Page(2, "XMPP")]
		[Header(19, "Trust Server Certificate")]
		[ToolTip(20, "If the server certificate should be trusted, even if it does not validate.")]
		[DefaultValue(false)]
		public bool TrustServer
		{
			get => this.trustServer;
			set => this.trustServer = value;
		}

		[Page(2, "XMPP")]
		[Header(21, "Allow Insecure Mechanisms")]
		[ToolTip(22, "If insecure mechanisms are permitted during authentication.")]
		[DefaultValue(false)]
		public bool AllowInsecureMechanisms
		{
			get => this.allowInsecureMechanisms;
			set => this.allowInsecureMechanisms = value;
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 23, "XMPP Broker");
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is XmppNode || Child is SourceNode || Child is PartitionNode || 
				Child is ConcentratorDevice);
		}

		public override Task DestroyAsync()
		{
			if (!string.IsNullOrEmpty(this.brokerKey))
				XmppBrokers.DestroyBroker(this.brokerKey);

			return base.DestroyAsync();
		}

		[IgnoreMember]
		public string Key
		{
			get
			{
				string PrevKey = this.brokerKey;
				this.brokerKey = XmppBrokers.GetKey(this.Host, this.Port, this.Tls, this.userName, this.password, this.passwordMechanism);

				if (PrevKey != this.brokerKey && !string.IsNullOrEmpty(PrevKey))
					XmppBrokers.DestroyBroker(PrevKey);

				return this.brokerKey;
			}
		}

		protected override Task NodeUpdated()
		{
			this.GetBroker();

			return base.NodeUpdated();
		}

		internal XmppBroker GetBroker()
		{
			return XmppBrokers.GetBroker(this, this.Key, this.Host, this.Port, this.Tls, this.userName, this.password,
				this.passwordMechanism, this.trustServer, this.allowInsecureMechanisms);
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
			XmppBroker Broker = this.GetBroker();

			Result.AddLast(new StringParameter("State", await Language.GetStringAsync(typeof(XmppBrokerNode), 27, "State"),
				Broker.Client.State.ToString() ?? string.Empty));

			return Result;
		}

	}
}
