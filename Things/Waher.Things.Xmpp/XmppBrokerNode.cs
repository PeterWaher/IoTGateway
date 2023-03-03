using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Persistence;
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
		private readonly Dictionary<CaseInsensitiveString, RosterItemNode> roster = new Dictionary<CaseInsensitiveString, RosterItemNode>();
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

		/// <summary>
		/// Partition ID
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(32, "Auto-accept Pattern:")]
		[ToolTip(33, "If a presence subscription comes from a JID that matches this regular expression, it will be automatically accepted.")]
		public string AutoAcceptPattern { get; set; }

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 23, "XMPP Broker");
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is XmppNode || Child is SourceNode || Child is PartitionNode ||
				Child is ConcentratorDevice || Child is RosterItemNode || Child is XmppExtensionNode);
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

		internal Task<XmppBroker> GetBroker()
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
			this.GetBroker().Result?.Client?.Add(Sniffer);
		}

		/// <summary>
		/// <see cref="ISniffable.AddRange"/>
		/// </summary>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.GetBroker().Result?.Client?.AddRange(Sniffers);
		}

		/// <summary>
		/// <see cref="ISniffable.Remove"/>
		/// </summary>
		public bool Remove(ISniffer Sniffer)
		{
			return this.GetBroker().Result?.Client?.Remove(Sniffer) ?? false;
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get { return this.GetBroker().Result?.Client?.Sniffers ?? new ISniffer[0]; }
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers
		{
			get { return this.GetBroker().Result?.Client?.HasSniffers ?? false; }
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
			return this.GetBroker().Result?.Client?.GetEnumerator() ?? new ISniffer[0].GetEnumerator();
		}

		#endregion

		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		public async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Result = new List<ICommand>();

			Result.AddRange(await base.Commands);
			Result.Add(new ReconnectCommand((await this.GetBroker()).Client));

			return Result;
		}

		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;
			XmppBroker Broker = await this.GetBroker();

			Result.AddLast(new StringParameter("State", await Language.GetStringAsync(typeof(XmppBrokerNode), 27, "State"),
				Broker.Client.State.ToString() ?? string.Empty));

			return Result;
		}

		#region Roster

		public async Task<RosterItemNode> GetRosterItem(string BareJID, bool CreateIfNotExists)
		{
			RosterItemNode Result;
			bool Load;

			lock (this.roster)
			{
				if (this.roster.TryGetValue(BareJID, out Result))
					return Result;

				Load = this.roster.Count == 0;
			}

			if (Load)
			{
				IEnumerable<INode> Children = await this.ChildNodes;

				lock (this.roster)
				{
					foreach (INode Node in Children)
					{
						if (Node is RosterItemNode RosterItem)
							this.roster[RosterItem.BareJID] = RosterItem;
					}

					if (this.roster.TryGetValue(BareJID, out Result))
						return Result;
				}
			}

			if (CreateIfNotExists)
			{
				Result = new RosterItemNode()
				{
					NodeId = await GetUniqueNodeId(this.NodeId + ", " + BareJID),
					BareJID = BareJID
				};

				await this.AddAsync(Result);

				return Result;
			}
			else
				return null;
		}

		public override async Task<bool> RemoveAsync(INode Child)
		{
			if (!await base.RemoveAsync(Child))
				return false;

			if (Child is RosterItemNode Item)
			{
				lock (this.roster)
				{
					this.roster.Remove(Item.BareJID);
				}
			}

			return true;
		}

		public override async Task AddAsync(INode Child)
		{
			await base.AddAsync(Child);

			if (Child is RosterItemNode Item)
			{
				lock (this.roster)
				{
					this.roster[Item.BareJID] = Item;
				}
			}
		}

		#endregion

	}
}
