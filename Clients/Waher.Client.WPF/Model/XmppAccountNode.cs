using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Logs;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model.Concentrator;
using Waher.Client.WPF.Model.Legal;
using Waher.Client.WPF.Model.Muc;
using Waher.Client.WPF.Model.Provisioning;
using Waher.Client.WPF.Model.PubSub;
using Waher.Client.WPF.Model.Things;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.XMPP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.P2P.SOCKS5;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.PEP.Events;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.RDP;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.Synchronization;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model
{
	public enum TransportMethod
	{
		TraditionalSocket = 0,
		WS = 1,
		BOSH = 2
	}

	/// <summary>
	/// Class representing a normal XMPP account.
	/// </summary>
	public class XmppAccountNode : TreeNode, IMenuAggregator, IHostReference
	{
		private const string SensorGroupName = "Sensors";
		private const string EventsGroupName = "Events";
		private const string ActuatorGroupName = "Actuators";
		private const string ConcentratorGroupName = "Concentrators";
		private const string OtherGroupName = "Others";
		private const string RemoteDesktopGroupName = "RDP";

		private readonly Dictionary<string, RemoteDesktopView> activeViews = [];
		private readonly LinkedList<KeyValuePair<DateTime, MessageEventArgs>> unhandledMessages = new();
		private readonly LinkedList<XmppComponent> components = new();
		private readonly Dictionary<string, List<EventHandlerAsync<RosterItem>>> rosterSubscriptions = new(StringComparer.CurrentCultureIgnoreCase);
		private readonly Connections connections;
		private EndpointSecurity e2eEncryption;
		private XmppClient client;
		private PepClient pepClient;
		private SensorClient sensorClient;
		private ControlClient controlClient;
		private ConcentratorClient concentratorClient;
		private SynchronizationClient synchronizationClient;
		private MultiUserChatClient mucClient;
		private RemoteDesktopClient rdpClient;
		private XmppEventReceptor eventReceptor;
		private Socks5Proxy socks5Proxy = null;
		//private XmppServerlessMessaging p2pNetwork;
		private DateTime connectionTimer = DateTime.MinValue;
		private Exception lastError = null;
		private TransportMethod transport = TransportMethod.TraditionalSocket;
		private string host;
		private string domain;
		private string urlBindResource;
		private int port;
		private string account;
		private string password;
		private string passwordHash;
		private string passwordHashMethod;
		private bool trustCertificate;
		private bool connected = false;
		private bool supportsSearch = false;
		private bool allowInsecureAuthentication = false;
		private readonly bool supportsHashes = true;

		/// <summary>
		/// Class representing a normal XMPP account.
		/// </summary>
		/// <param name="Connections">Connections object.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Host">Host name.</param>
		/// <param name="Transport">Transport method.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="UrlBindResource">URL bind resource.</param>
		/// <param name="Account">Account name.</param>
		/// <param name="PasswordHash">Password hash.</param>
		/// <param name="PasswordHashMethod">Password hash method.</param>
		/// <param name="TrustCertificate">If the server certificate should be trusted.</param>
		/// <param name="AllowInsecureAuthentication">If insecure authentication mechanisms are to be allowed.</param>
		public XmppAccountNode(Connections Connections, TreeNode Parent, string Host, TransportMethod Transport, int Port, string UrlBindResource,
			string Account, string PasswordHash, string PasswordHashMethod, bool TrustCertificate, bool AllowInsecureAuthentication)
			: base(Parent)
		{
			this.connections = Connections;
			this.host = this.domain = Host;
			this.transport = Transport;
			this.port = Port;
			this.urlBindResource = UrlBindResource;
			this.account = Account;

			if (string.IsNullOrEmpty(PasswordHashMethod))
			{
				this.password = PasswordHash;
				this.passwordHash = string.Empty;
				this.passwordHashMethod = string.Empty;
				this.supportsHashes = false;
			}
			else
			{
				this.password = string.Empty;
				this.passwordHash = PasswordHash;
				this.passwordHashMethod = PasswordHashMethod;
				this.supportsHashes = true;
			}

			this.trustCertificate = TrustCertificate;
			this.allowInsecureAuthentication = AllowInsecureAuthentication;

			this.Init();
		}

		public XmppAccountNode(XmlElement E, Connections Connections, TreeNode Parent)
			: base(Parent)
		{
			this.connections = Connections;
			this.host = XML.Attribute(E, "host");
			this.transport = XML.Attribute(E, "transport", TransportMethod.TraditionalSocket);
			this.urlBindResource = XML.Attribute(E, "urlBindResource");
			this.domain = XML.Attribute(E, "domain", this.host);
			this.port = XML.Attribute(E, "port", XmppCredentials.DefaultPort);
			this.account = XML.Attribute(E, "account");
			this.password = XML.Attribute(E, "password");
			this.passwordHash = XML.Attribute(E, "passwordHash");
			this.passwordHashMethod = XML.Attribute(E, "passwordHashMethod");
			this.trustCertificate = XML.Attribute(E, "trustCertificate", false);
			this.allowInsecureAuthentication = XML.Attribute(E, "allowInsecureAuthentication", false);
			this.supportsHashes = XML.Attribute(E, "supportsHashes", true);

			this.Init();
		}

		private void Init(params ISniffer[] Sniffers)
		{
			XmppCredentials Credentials = new()
			{
				Host = this.host,
				Port = this.port,
				Account = this.account,
				TrustServer = this.trustCertificate,
				AllowPlain = this.allowInsecureAuthentication,
				AllowCramMD5 = this.allowInsecureAuthentication,
				AllowDigestMD5 = this.allowInsecureAuthentication
			};

			switch (this.transport)
			{
				case TransportMethod.BOSH:
				case TransportMethod.WS:
					Credentials.UriEndpoint = this.urlBindResource;
					break;
			}

			if (!string.IsNullOrEmpty(this.passwordHash))
			{
				Credentials.Password = this.passwordHash;
				Credentials.PasswordType = this.passwordHashMethod;
			}
			else
				Credentials.Password = this.password;

			this.client = new XmppClient(Credentials, "en", typeof(App).Assembly)
			{
				AllowQuickLogin = true,
			};

			if (Sniffers is not null)
				this.client.AddRange(Sniffers);

			this.client.OnStateChanged += this.Client_OnStateChanged;
			this.client.OnError += this.Client_OnError;
			this.client.OnPresence += this.Client_OnPresence;
			this.client.OnPresenceSubscribe += this.Client_OnPresenceSubscribe;
			this.client.OnPresenceUnsubscribe += this.Client_OnPresenceUnsubscribe;
			this.client.OnRosterItemAdded += this.Client_OnRosterItemUpdated;
			this.client.OnRosterItemRemoved += this.Client_OnRosterItemRemoved;
			this.client.OnRosterItemUpdated += this.Client_OnRosterItemUpdated;
			this.connectionTimer = MainWindow.Scheduler.Add(DateTime.Now.AddMinutes(1), this.CheckConnection, null);
			this.client.OnNormalMessage += this.Client_OnNormalMessage;
			this.client.OnErrorMessage += this.Client_OnErrorMessage;

			this.client.SetPresence(Availability.Chat);

			this.sensorClient = new SensorClient(this.client);
			this.controlClient = new ControlClient(this.client);
			this.concentratorClient = new ConcentratorClient(this.client);
			this.synchronizationClient = new SynchronizationClient(this.client);

			this.eventReceptor = new XmppEventReceptor(this.client);
			this.eventReceptor.OnEvent += this.EventReceptor_OnEvent;

			this.AddPepClient(string.Empty);

			this.concentratorClient.OnCustomSnifferMessage += this.ConcentratorClient_OnCustomSnifferMessage;
			this.concentratorClient.OnEvent += this.ConcentratorClient_OnEvent;

			//this.p2pNetwork = new XmppServerlessMessaging("RDP " + this.BareJID, this.client.BareJID);
			//this.p2pNetwork.OnNewXmppClient += ServerlessMessaging_OnNewXmppClient;
			//this.p2pNetwork.OnResynch += ServerlessMessaging_OnResynch;

			this.e2eEncryption = new EndpointSecurity(this.client, /*this.p2pNetwork,*/ 128,
				new Edwards25519Endpoint(), new Edwards448Endpoint());
			this.client.SetTag("E2E", this.e2eEncryption);

			this.socks5Proxy = new Socks5Proxy(this.client);  //, this.XmppAccountNode.E2E);		TODO
			this.socks5Proxy.OnOpen += this.Proxy_OnOpen;

			this.rdpClient = new RemoteDesktopClient(this.client, this.e2eEncryption);

			this.client.Connect();
		}

		private Task Client_OnErrorMessage(object _, MessageEventArgs e)
		{
			string Msg = e.ErrorText;
			if (!string.IsNullOrEmpty(Msg))
				MainWindow.ErrorBox(Msg);

			return Task.CompletedTask;
		}

		private void AddPepClient(string PubSubComponentAddress)
		{
			this.pepClient?.Dispose();
			this.pepClient = null;

			this.pepClient = new PepClient(this.client, PubSubComponentAddress);

			this.pepClient.OnUserActivity += this.PepClient_OnUserActivity;
			this.pepClient.OnUserAvatarMetaData += this.PepClient_OnUserAvatarMetaData;
			this.pepClient.OnUserLocation += this.PepClient_OnUserLocation;
			this.pepClient.OnUserMood += this.PepClient_OnUserMood;
			this.pepClient.OnUserTune += this.PepClient_OnUserTune;
			this.pepClient.RegisterHandler(typeof(SensorData), this.PepClient_SensorData);
		}

		private void AddMucClient(string MucComponentAddress)
		{
			this.mucClient?.Dispose();
			this.mucClient = null;

			this.mucClient = new MultiUserChatClient(this.client, MucComponentAddress);
		}

		private Task EventReceptor_OnEvent(object _, EventEventArgs e)
		{
			MainWindow.UpdateGui(() =>
			{
				LogView View = MainWindow.currentInstance.GetLogView(e.FromBareJID);
				View.Add(new LogItem(e.Event));
				return Task.CompletedTask;
			});

			return Task.CompletedTask;
		}

		private async Task ConcentratorClient_OnCustomSnifferMessage(object _, CustomSnifferEventArgs e)
		{
			TaskCompletionSource<SnifferView> View = new();

			MainWindow.UpdateGui(() =>
			{
				View.TrySetResult(MainWindow.currentInstance.GetSnifferView(null, e.FromBareJID, true));
				return Task.CompletedTask;
			});

			SnifferView SnifferView = await View.Task;

			e.Sniffer = SnifferView.Sniffer;
		}

		private async Task ConcentratorClient_OnEvent(object Sender, SourceEventMessageEventArgs EventMessage)
		{
			if (this.TryGetChild(EventMessage.FromBareJID, out TreeNode Child) &&
				(Child is XmppConcentrator Concentrator))
			{
				await Concentrator.ConcentratorClient_OnEvent(Sender, EventMessage);
			}
		}

		private Task Client_OnNormalMessage(object _, MessageEventArgs e)
		{
			DateTime Now = DateTime.Now;
			DateTime Limit = Now.AddMinutes(-1);

			lock (this.unhandledMessages)
			{
				this.unhandledMessages.AddLast(new KeyValuePair<DateTime, MessageEventArgs>(Now, e));

				while (this.unhandledMessages.First is not null && this.unhandledMessages.First.Value.Key <= Limit)
					this.unhandledMessages.RemoveFirst();
			}

			return Task.CompletedTask;
		}

		public IEnumerable<MessageEventArgs> GetUnhandledMessages(string LocalName, string Namespace)
		{
			LinkedList<MessageEventArgs> Result = new();
			LinkedListNode<KeyValuePair<DateTime, MessageEventArgs>> Loop, Next;
			bool Found;

			lock (this.unhandledMessages)
			{
				Loop = this.unhandledMessages.First;

				while (Loop is not null)
				{
					Next = Loop.Next;

					Found = false;

					foreach (XmlElement E in Loop.Value.Value.Message.ChildNodes)
					{
						if (E.LocalName == LocalName && E.NamespaceURI == Namespace)
						{
							Found = true;
							break;
						}
					}

					if (Found)
					{
						Result.AddLast(Loop.Value.Value);
						this.unhandledMessages.Remove(Loop);
					}

					Loop = Next;
				}
			}

			return Result;
		}

		private Task Client_OnError(object _, Exception Exception)
		{
			this.lastError = Exception;
			return Task.CompletedTask;
		}

		private async Task Client_OnStateChanged(object _, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected = true;
					this.lastError = null;

					if (this.supportsHashes && string.IsNullOrEmpty(this.passwordHash))
					{
						this.passwordHash = this.client.PasswordHash;
						this.passwordHashMethod = this.client.PasswordHashMethod;
						this.connections.Modified = true;
					}

					if (this.domain != this.client.Domain)
					{
						this.domain = this.client.Domain;
						this.connections.Modified = true;
					}

					this.CheckRoster();
					this.SearchComponents();

					//this.p2pNetwork.FullJid = this.client.FullJID;
					break;

				case XmppState.Offline:
					bool ImmediateReconnect = this.connected;
					this.connected = false;

					if (ImmediateReconnect && this.client is not null)
						await this.client.Reconnect();
					break;
			}

			this.OnUpdated();
		}

		public TransportMethod Transport => this.transport;
		public string Host => this.host;
		public int Port => this.port;
		public string Account => this.account;
		public string PasswordHash => this.passwordHash;
		public string PasswordHashMethod => this.passwordHashMethod;
		public bool TrustCertificate => this.trustCertificate;
		public bool AllowInsecureAuthentication => this.allowInsecureAuthentication;
		public bool SupportsHashes => this.supportsHashes;

		public override string Header
		{
			get { return this.account + "@" + this.domain; }
		}

		public override string TypeName
		{
			get { return "XMPP Account"; }
		}

		public override void Dispose()
		{
			base.Dispose();

			if (this.connectionTimer > DateTime.MinValue)
			{
				MainWindow.Scheduler?.Remove(this.connectionTimer);
				this.connectionTimer = DateTime.MinValue;
			}

			this.rdpClient?.Dispose();
			this.rdpClient = null;

			this.pepClient?.Dispose();
			this.pepClient = null;

			this.mucClient?.Dispose();
			this.mucClient = null;

			this.sensorClient?.Dispose();
			this.sensorClient = null;

			this.controlClient?.Dispose();
			this.controlClient = null;

			this.concentratorClient?.Dispose();
			this.concentratorClient = null;

			this.synchronizationClient?.Dispose();
			this.synchronizationClient = null;

			this.e2eEncryption?.Dispose();
			this.e2eEncryption = null;

			this.eventReceptor?.Dispose();
			this.eventReceptor = null;

			if (this.client is not null)
			{
				this.client.RemoveTag("E2E");

				XmppClient Client = this.client;
				this.client = null;
				Client.OfflineAndDisposeAsync().Wait();	// TODO: Asynchronous
			}
		}

		private async void CheckConnection(object P)
		{
			try
			{
				this.connectionTimer = MainWindow.Scheduler.Add(DateTime.Now.AddMinutes(1), this.CheckConnection, null);

				if (this.client is not null && (this.client.State == XmppState.Offline || this.client.State == XmppState.Error || this.client.State == XmppState.Authenticating))
					await this.client.Reconnect();
			}
			catch (Exception ex)
			{
				MessageBox.Show(MainWindow.currentInstance, ex.Message, "Unable to reconnect.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public override void Write(XmlWriter Output)
		{
			Output.WriteStartElement("XmppAccount");
			Output.WriteAttributeString("host", this.host);
			Output.WriteAttributeString("domain", this.domain);
			Output.WriteAttributeString("transport", this.transport.ToString());
			Output.WriteAttributeString("port", this.port.ToString());
			Output.WriteAttributeString("urlBindResource", this.urlBindResource);

			Output.WriteAttributeString("account", this.account);

			if (string.IsNullOrEmpty(this.passwordHash))
				Output.WriteAttributeString("password", this.password);
			else
			{
				Output.WriteAttributeString("passwordHash", this.passwordHash);
				Output.WriteAttributeString("passwordHashMethod", this.passwordHashMethod);
			}

			Output.WriteAttributeString("trustCertificate", CommonTypes.Encode(this.trustCertificate));
			Output.WriteAttributeString("allowInsecureAuthentication", CommonTypes.Encode(this.allowInsecureAuthentication));

			if (!this.supportsHashes)
				Output.WriteAttributeString("supportsHashes", CommonTypes.Encode(this.supportsHashes));

			Output.WriteEndElement();
		}

		internal static readonly BitmapImage away = new(new Uri("../Graphics/Away.png", UriKind.Relative));
		internal static readonly BitmapImage busy = new(new Uri("../Graphics/DoNotDisturb.png", UriKind.Relative));
		internal static readonly BitmapImage chat = new(new Uri("../Graphics/Chat.png", UriKind.Relative));
		internal static readonly BitmapImage extendedAway = new(new Uri("../Graphics/ExtendedAway.png", UriKind.Relative));
		internal static readonly BitmapImage offline = new(new Uri("../Graphics/Offline.png", UriKind.Relative));
		internal static readonly BitmapImage online = new(new Uri("../Graphics/Online.png", UriKind.Relative));
		internal static readonly BitmapImage folderBlueClosed = new(new Uri("../Graphics/folder-blue-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderBlueOpen = new(new Uri("../Graphics/folder-blue-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderYellowClosed = new(new Uri("../Graphics/folder-yellow-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderYellowOpen = new(new Uri("../Graphics/folder-yellow-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage box = new(new Uri("../Graphics/App-miscellaneous-icon.png", UriKind.Relative));
		internal static readonly BitmapImage hourglass = new(new Uri("../Graphics/hourglass-icon.png", UriKind.Relative));
		internal static readonly BitmapImage database = new(new Uri("../Graphics/Database-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage component = new(new Uri("../Graphics/server-components-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage chatBubble = new(new Uri("../Graphics/Chat-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage legal = new(new Uri("../Graphics/justice-balance-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage log = new(new Uri("../Graphics/App-edit-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage none = new(new Uri("../Graphics/None.png", UriKind.Relative));
		internal static readonly BitmapImage from = new(new Uri("../Graphics/From.png", UriKind.Relative));
		internal static readonly BitmapImage to = new(new Uri("../Graphics/To.png", UriKind.Relative));
		internal static readonly BitmapImage both = new(new Uri("../Graphics/Both.png", UriKind.Relative));

		public override ImageSource ImageResource
		{
			get
			{
				if (this.client is null)
					return offline;
				else
				{
					return this.client.State switch
					{
						XmppState.Connected => online,
						XmppState.Error => busy,
						_ => offline,
					};
				}
			}
		}

		public override string ToolTip
		{
			get
			{
				switch (this.client.State)
				{
					case XmppState.Offline:
					default:
						return "Offline";

					case XmppState.Connecting:
						return "Connecting to broker.";

					case XmppState.StreamNegotiation:
						return "Performing Stream Negotiation.";

					case XmppState.StreamOpened:
						return "Stream Opened.";

					case XmppState.StartingEncryption:
						return "Switching to encrypted channel.";

					case XmppState.Authenticating:
						return "Performing user authentication.";

					case XmppState.Registering:
						return "Registering user account.";

					case XmppState.Binding:
						return "Performing session binding.";

					case XmppState.FetchingRoster:
						return "Fetching roster.";

					case XmppState.SettingPresence:
						return "Setting presence.";

					case XmppState.Connected:
						return "Connected.";

					case XmppState.Error:
						if (this.lastError is null)
							return "In an error state.";
						else
							return this.lastError.Message;
				}
			}
		}

		public override bool CanAddChildren
		{
			get
			{
				return this.client is not null && this.client.State == XmppState.Connected;
			}
		}

		public override bool CanEdit => true;
		public override bool CanDelete => true;

		public override void Add()
		{
			AddContactForm Dialog = new()
			{
				Owner = this.connections.Owner
			};

			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
				this.client.RequestPresenceSubscription(Dialog.ContactJID.Text);
		}

		private void CheckRoster()
		{
			SortedDictionary<string, TreeNode> Contacts = this.children;
			Dictionary<string, TreeNode> Existing = [];
			LinkedList<TreeNode> Added = null;
			LinkedList<KeyValuePair<string, TreeNode>> Removed = null;
			LinkedList<RosterItem> Resubscribe = null;
			LinkedList<RosterItem> Reunsubscribe = null;

			Contacts ??= [];

			lock (Contacts)
			{
				foreach (RosterItem Item in this.client.Roster)
				{
					if (Contacts.TryGetValue(Item.BareJid, out TreeNode Contact))
						Existing[Item.BareJid] = Contact;
					else
					{
						if (Item.IsInGroup(ConcentratorGroupName))
							Contact = new XmppConcentrator(this, this.client, Item.BareJid, Item.IsInGroup(EventsGroupName), Item.IsInGroup(RemoteDesktopGroupName));
						else if (Item.IsInGroup(ActuatorGroupName))
							Contact = new XmppActuator(this, this.client, Item.BareJid, Item.IsInGroup(SensorGroupName), Item.IsInGroup(EventsGroupName), Item.IsInGroup(RemoteDesktopGroupName));
						else if (Item.IsInGroup(SensorGroupName))
							Contact = new XmppSensor(this, this.client, Item.BareJid, Item.IsInGroup(EventsGroupName), Item.IsInGroup(RemoteDesktopGroupName));
						else if (Item.IsInGroup(OtherGroupName))
							Contact = new XmppOther(this, this.client, Item.BareJid, Item.IsInGroup(RemoteDesktopGroupName));
						else
							Contact = new XmppContact(this, this.client, Item.BareJid, Item.IsInGroup(RemoteDesktopGroupName));

						Contacts[Item.BareJid] = Contact;

						Added ??= [];
						Added.AddLast(Contact);
					}

					switch (Item.PendingSubscription)
					{
						case PendingSubscription.Subscribe:
							Resubscribe ??= [];
							Resubscribe.AddLast(Item);
							break;

						case PendingSubscription.Unsubscribe:
							Reunsubscribe ??= [];
							Reunsubscribe.AddLast(Item);
							break;
					}
				}

				if (this.children is null)
					this.children = Contacts;
				else
				{
					foreach (KeyValuePair<string, TreeNode> P in this.children)
					{
						if (P.Value is XmppContact Contact &&
							!Existing.ContainsKey(Contact.BareJID))
						{
							Removed ??= new LinkedList<KeyValuePair<string, TreeNode>>();
							Removed.AddLast(P);
						}
					}

					if (Removed is not null)
					{
						foreach (KeyValuePair<string, TreeNode> P in Removed)
							this.children.Remove(P.Key);
					}
				}
			}

			if (Added is not null)
			{
				foreach (TreeNode Node in Added)
					this.connections.Owner.MainView.NodeAdded(this, Node);
			}

			if (Removed is not null)
			{
				foreach (KeyValuePair<string, TreeNode> P in Removed)
					this.connections.Owner.MainView.NodeRemoved(this, P.Value);
			}

			if (Resubscribe is not null)
			{
				foreach (RosterItem Item in Resubscribe)
					this.client.RequestPresenceSubscription(Item.BareJid);
			}

			if (Reunsubscribe is not null)
			{
				foreach (RosterItem Item in Reunsubscribe)
					this.client.RequestPresenceUnsubscription(Item.BareJid);
			}

			this.OnUpdated();
		}

		public Controls.ConnectionView View
		{
			get { return this.connections.Owner.MainView; }
		}

		private async Task Client_OnRosterItemUpdated(object _, RosterItem Item)
		{
			if (this.children is null)
				this.CheckRoster();
			else
			{
				XmppContact Contact;
				bool Added = false;

				lock (this.children)
				{
					if (this.children.TryGetValue(Item.BareJid, out TreeNode Node))
					{
						if ((Contact = Node as XmppContact) is not null)
							Contact.RosterItem = Item;
					}
					else
					{
						Contact = new XmppContact(this, this.client, Item.BareJid, Item.IsInGroup(RemoteDesktopGroupName));
						this.children[Item.BareJid] = Contact;
						Added = true;
					}
				}

				if (Added)
				{
					this.connections.Owner.MainView.NodeAdded(this, Contact);
					this.OnUpdated();
				}
				else
					Contact.OnUpdated();

				await this.CheckRosterItemSubscriptions(Item);
			}
		}

		private async Task CheckRosterItemSubscriptions(RosterItem Item)
		{
			EventHandlerAsync<RosterItem>[] h;

			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(Item.BareJid, out List<EventHandlerAsync<RosterItem>> List))
					h = [.. List];
				else
					h = null;
			}

			if (h is not null)
			{
				foreach (EventHandlerAsync<RosterItem> h2 in h)
					await h2.Raise(this, Item);
			}
		}

		public void RegisterRosterEventHandler(string BareJid, EventHandlerAsync<RosterItem> Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (!this.rosterSubscriptions.TryGetValue(BareJid, out List<EventHandlerAsync<RosterItem>> h))
				{
					h = [];
					this.rosterSubscriptions[BareJid] = h;
				}

				h.Add(Callback);
			}
		}

		public void UnregisterRosterEventHandler(string BareJid, EventHandlerAsync<RosterItem> Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(BareJid, out List<EventHandlerAsync<RosterItem>> h) && h.Remove(Callback) && h.Count == 0)
					this.rosterSubscriptions.Remove(BareJid);
			}
		}

		private Task Client_OnRosterItemRemoved(object _, RosterItem Item)
		{
			if (this.children is null)
				this.CheckRoster();
			else
			{
				bool Updated;

				lock (this.children)
				{
					Updated = this.children.Remove(Item.BareJid);
				}

				this.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private async Task Client_OnPresence(object _, PresenceEventArgs e)
		{
			if (this.children is null)
				this.CheckRoster();
			else
			{
				TreeNode Node;

				lock (this.children)
				{
					if (!this.children.TryGetValue(e.FromBareJID, out Node))
						Node = null;
				}

				if (Node is not null)
				{
					Node.OnUpdated();

					if (e.Availability != Availability.Offline && Node.GetType() == typeof(XmppContact))
						this.CheckType(Node, e.From);
				}
				else if (string.Compare(e.FromBareJID, this.client.BareJID, true) == 0)
					this.client.Information("Presence from same bare JID. Ignored.");
				else
					this.client.Warning("Presence from node not found in roster: " + e.FromBareJID);

				RosterItem Item = this.client?.GetRosterItem(e.FromBareJID);
				if (Item is not null)
					await this.CheckRosterItemSubscriptions(Item);
			}
		}

		internal void CheckType(TreeNode Node, string FullJid)
		{
			this.client.SendServiceDiscoveryRequest(FullJid, this.ServiceDiscoveryResponse, Node);
		}

		private Task ServiceDiscoveryResponse(object _, ServiceDiscoveryEventArgs e)
		{
			if (e.Ok)
			{
				XmppContact Node = (XmppContact)e.State;
				object OldTag;

				if (e.HasAnyFeature(ConcentratorServer.NamespacesConcentrator))
				{
					bool SupportsEvents = e.HasAnyFeature(SensorClient.NamespacesSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppConcentrator(Node.Parent, this.client, Node.BareJID, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups =
					[
						ConcentratorGroupName
					];

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, [.. Groups]);
				}
				else if (e.HasAnyFeature(ControlClient.NamespacesControl))
				{
					bool IsSensor = e.HasAnyFeature(SensorClient.NamespacesSensorData);
					bool SupportsEvents = e.HasAnyFeature(SensorClient.NamespacesSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppActuator(Node.Parent, this.client, Node.BareJID, IsSensor, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups =
					[
						ActuatorGroupName
					];

					if (IsSensor)
						Groups.Add(SensorGroupName);

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, [.. Groups]);
				}
				else if (e.HasAnyFeature(SensorClient.NamespacesSensorData))
				{
					bool SupportsEvents = e.HasAnyFeature(SensorClient.NamespacesSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppSensor(Node.Parent, this.client, Node.BareJID, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups =
					[
						SensorGroupName
					];

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, [.. Groups]);
				}
				else
				{
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppOther(Node.Parent, this.client, Node.BareJID, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups =
					[
						OtherGroupName
					];

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, [.. Groups]);
				}

				this.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private void AddGroups(XmppContact Contact, params string[] GroupNames)
		{
			string[] Groups = Contact.RosterItem.Groups;
			bool Updated = false;
			int c;

			foreach (string GroupName in GroupNames)
			{
				if (Array.IndexOf<string>(Groups, GroupName) < 0)
				{
					c = Groups.Length;
					Array.Resize<string>(ref Groups, c + 1);
					Groups[c] = GroupName;

					Updated = true;
				}
			}

			if (Updated)
			{
				Array.Sort<string>(Groups);
				this.client.UpdateRosterItem(Contact.BareJID, Contact.RosterItem.Name, Groups);
			}
		}

		private async Task Client_OnPresenceSubscribe(object _, PresenceEventArgs e)
		{
			RosterItem Item = e.Client[e.FromBareJID];

			if (Item is not null && (Item.State == SubscriptionState.Both || Item.State == SubscriptionState.From))
				await e.Accept();
			else
				MainWindow.UpdateGui(this.PresenceSubscribe, e);
		}

		private async Task PresenceSubscribe(object P)
		{
			PresenceEventArgs e = (PresenceEventArgs)P;

			switch (MessageBox.Show(this.connections.Owner, e.FromBareJID + " has requested to subscribe to your presence (become your friend). Do you accept?",
				this.client.BareJID, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes))
			{
				case MessageBoxResult.Yes:
					await e.Accept();

					RosterItem Item = this.client.GetRosterItem(e.FromBareJID);
					if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
						await this.client.RequestPresenceSubscription(e.FromBareJID);

					await this.client.SetPresence(Availability.Chat);
					break;

				case MessageBoxResult.No:
					await e.Decline();
					break;

				case MessageBoxResult.Cancel:
				default:
					// Do nothing.
					break;
			}
		}

		private Task Client_OnPresenceUnsubscribe(object _, PresenceEventArgs e)
		{
			return e.Accept();
		}

		public override bool CanRecycle
		{
			get { return this.client is not null; }
		}

		public override async Task Recycle(MainWindow Window)
		{
			if (this.children is not null)
			{
				foreach (TreeNode Node in this.children.Values)
				{
					if (Node.CanRecycle)
						await Node.Recycle(Window);
				}
			}

			await this.client.Reconnect();
		}

		public bool IsOnline
		{
			get
			{
				return this.client is not null && this.client.State == XmppState.Connected;
			}
		}

		public string BareJID
		{
			get
			{
				if (this.client is null)
					return string.Empty;
				else
					return this.client.BareJID;
			}
		}

		public override string Key => this.BareJID;
		public override bool IsSniffable => this.client is not null;
		public XmppClient Client => this.client;
		//public XmppServerlessMessaging P2P => this.p2pNetwork;
		public EndpointSecurity E2E => this.e2eEncryption;
		public PepClient PepClient => this.pepClient;
		public MultiUserChatClient MucClient => this.mucClient;
		public SensorClient SensorClient => this.sensorClient;
		public ControlClient ControlClient => this.controlClient;
		public ConcentratorClient ConcentratorClient => this.concentratorClient;
		public SynchronizationClient SynchronizationClient => this.synchronizationClient;
		public RemoteDesktopClient RdpClient => this.rdpClient;

		public override bool RemoveChild(TreeNode Node)
		{
			if (base.RemoveChild(Node))
			{
				if (Node is XmppContact Contact)
				{
					try
					{
						this.client.RemoveRosterItem(Contact.BareJID);
					}
					catch (ArgumentException)
					{
						// Already removed.
					}
				}

				return true;
			}
			else
				return false;
		}

		public override void AddSniffer(Networking.Sniffers.ISniffer Sniffer)
		{
			this.client.Add(Sniffer);
		}

		public override Task<bool> RemoveSniffer(ISniffer Sniffer)
		{
			if (this.client is null)
				return Task.FromResult(false);
			else
				return Task.FromResult(this.client.Remove(Sniffer));
		}

		public override void Added(MainWindow Window)
		{
			this.client.OnChatMessage += Window.OnChatMessage;
			this.client.OnStateChanged += Window.OnStateChange;
		}

		public override void Removed(MainWindow Window)
		{
			this.client.OnChatMessage -= Window.OnChatMessage;
			this.client.OnStateChanged -= Window.OnStateChange;
		}

		public void SearchComponents()
		{
			this.client.SendServiceDiscoveryRequest(this.client.Domain, (Sender, e) =>
			{
				this.supportsSearch = e.HasFeature(XmppClient.NamespaceSearch);

				if (!this.supportsSearch)
				{
					this.client.SendSearchFormRequest(string.Empty, (sender2, e2) =>
					{
						if (e2.Ok)
							this.supportsSearch = true;

						return Task.CompletedTask;

					}, null, null);
				}

				return Task.CompletedTask;

			}, null);

			bool SupportsDiscovery = false;
			bool SupportsLegal = false;

			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, (Sender, e) =>
			{
				foreach (Item Item in e.Items)
				{
					this.client.SendServiceDiscoveryRequest(Item.JID, async (sender2, e2) =>
					{
						try
						{
							XmppComponent Component = null;
							ThingRegistry ThingRegistry = null;

							this.children ??= [];

							lock (this.children)
							{
								if (this.children.ContainsKey(Item.JID))
									return;
							}

							if (e2.HasAnyFeature(ThingRegistryClient.NamespacesDiscovery))
							{
								if (SupportsDiscovery)
									return;

								SupportsDiscovery = true;
								ThingRegistry = new ThingRegistry(this, Item.JID, Item.Name, Item.Node, e2.Features);
								Component = ThingRegistry;
							}
							else if (e2.HasFeature(PubSubClient.NamespacePubSub))
							{
								this.AddPepClient(Item.JID);
								Component = new PubSubService(this, Item.JID, Item.Name, Item.Node, e2.Features, this.pepClient.PubSubClient);
							}
							else if (e2.HasFeature(MultiUserChatClient.NamespaceMuc))
							{
								this.AddMucClient(Item.JID);
								Component = new MucService(this, Item.JID, Item.Name, Item.Node, e2.Features, this.mucClient);
							}
							else if (e2.HasAnyFeature(ContractsClient.NamespacesLegalIdentities))
							{
								if (SupportsLegal)
									return;

								SupportsLegal = true;
								Component = await LegalService.Create(this, Item.JID, Item.Name, 
									Item.Node, this.e2eEncryption, e2.Features);
							}
							else if (e2.HasFeature(EventLog.NamespaceEventLogging))
								Component = new EventLog(this, Item.JID, Item.Name, Item.Node, e2.Features);
							else
								Component = new XmppComponent(this, Item.JID, Item.Name, Item.Node, e2.Features);

							lock (this.children)
							{
								if (this.children.ContainsKey(Item.JID))
								{
									Component.Dispose();
									return;
								}

								this.children[Item.JID] = Component;
							}

							this.connections.Owner.MainView.NodeAdded(this, Component);
							this.OnUpdated();

							if (ThingRegistry is not null && ThingRegistry.SupportsProvisioning)
							{
								MainWindow.UpdateGui(() =>
								{
									MainWindow.currentInstance.NewQuestion(this, ThingRegistry.ProvisioningClient, null);
									return Task.CompletedTask;
								});
							}
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}

					}, null);
				}

				return Task.CompletedTask;

			}, null);
		}

		public override bool CanConfigure => this.IsOnline;

		public override async Task GetConfigurationForm(EventHandlerAsync<DataFormEventArgs> Callback, object State)
		{
			DataForm Form = new(this.client, this.ChangePassword, this.CancelChangePassword, this.BareJID, this.BareJID,
				new TextPrivateField(null, "Password", "New password:", true, [string.Empty], null,
					"Enter new password here.", new StringDataType(), new PasswordValidation(), string.Empty, false, false, false),
				new TextPrivateField(null, "Password2", "Retype password:", true, [string.Empty], null,
					"Retype password here.", new StringDataType(), new Password2Validation(), string.Empty, false, false, false))
			{
				Title = "Change password",
				Instructions = ["Enter the new password you wish to use."]
			};

			await Callback.Raise(this, new DataFormEventArgs(Form, new IqResultEventArgs(null, string.Empty, this.BareJID, this.BareJID, true, State)));
		}

		private class PasswordValidation : BasicValidation
		{
			public override void Validate(Networking.XMPP.DataForms.Field Field, DataType DataType, object[] Parsed, string[] Strings)
			{
				string Password = Strings[0];

				if (Password.Length < 6)
					Field.Error = "Password too short.";
				else
				{
					bool Digits = false;
					bool Lower = false;
					bool Upper = false;

					foreach (char ch in Password)
					{
						Digits |= char.IsDigit(ch);
						Lower |= char.IsLower(ch);
						Upper |= char.IsUpper(ch);
					}

					if (!Digits)
						Field.Error = "Password must contain digits.";
					else if (!Lower)
						Field.Error = "Password must contain lower case characters.";
					else if (!Upper)
						Field.Error = "Password must contain upper case characters.";
				}
			}
		}

		private class Password2Validation : BasicValidation
		{
			public override void Validate(Networking.XMPP.DataForms.Field Field, DataType DataType, object[] Parsed, string[] Strings)
			{
				string Password = Strings[0];

				if (Password != Field.Form["Password"].ValueString)
					Field.Error = "Passwords don't match.";
			}
		}

		private Task ChangePassword(object _, DataForm Form)
		{
			string NewPassword = Form["Password"].ValueString;

			this.client.ChangePassword(NewPassword, (Sender, e) =>
			{
				if (e.Ok)
				{
					this.connections.Modified = true;
					this.passwordHash = string.Empty;
					this.client.Reconnect(this.client.UserName, NewPassword);

					MainWindow.SuccessBox("Password successfully changed.");
				}
				else
					MainWindow.ErrorBox("Unable to change password.");

				return Task.CompletedTask;

			}, null);

			return Task.CompletedTask;
		}

		private Task CancelChangePassword(object _, DataForm _2)
		{
			// Do nothing.
			return Task.CompletedTask;
		}

		public override bool CanReadSensorData => this.IsOnline;

		public override Task<SensorDataClientRequest> StartSensorDataFullReadout()
		{
			return this.DoReadout(FieldType.All);
		}

		public override Task<SensorDataClientRequest> StartSensorDataMomentaryReadout()
		{
			return this.DoReadout(FieldType.Momentary);
		}

		private async Task<SensorDataClientRequest> DoReadout(FieldType Types)
		{
			string Id = Guid.NewGuid().ToString();

			CustomSensorDataClientRequest Request = new(Id, string.Empty, string.Empty, null,
				Types, null, DateTime.MinValue, DateTime.MaxValue, DateTime.Now, string.Empty, string.Empty, string.Empty);

			await Request.Accept(false);
			await Request.Started();

			await this.client.SendServiceDiscoveryRequest(string.Empty, (Sender, e) =>
			{
				if (e.Ok)
				{
					List<Waher.Things.SensorData.Field> Fields = [];
					DateTime Now = DateTime.Now;

					foreach (KeyValuePair<string, bool> Feature in e.Features)
					{
						Fields.Add(new Waher.Things.SensorData.BooleanField(Waher.Things.ThingReference.Empty, Now,
							Feature.Key, Feature.Value, FieldType.Momentary, FieldQoS.AutomaticReadout));
					}

					bool VersionDone = false;

					if ((Types & FieldType.Identity) != 0)
					{
						foreach (Identity Identity in e.Identities)
						{
							Fields.Add(new StringField(Waher.Things.ThingReference.Empty, Now,
								Identity.Type, Identity.Category + (string.IsNullOrEmpty(Identity.Name) ? string.Empty : " (" + Identity.Name + ")"),
								FieldType.Identity,
								FieldQoS.AutomaticReadout));
						}

						if (e.HasFeature(XmppClient.NamespaceSoftwareVersion))
						{
							this.client.SendSoftwareVersionRequest(string.Empty, (sender2, e2) =>
							{
								Now = DateTime.Now;

								if (e2.Ok)
								{
									Request.LogFields(
									[
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, Name", e2.Name,
											FieldType.Identity, FieldQoS.AutomaticReadout),
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, OS", e2.OS,
											FieldType.Identity, FieldQoS.AutomaticReadout),
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, Version", e2.Version,
											FieldType.Identity, FieldQoS.AutomaticReadout),
									]);
								}
								else
								{
									Request.LogErrors(
									[
										new(Waher.Things.ThingReference.Empty, Now, "Unable to read software version.")
									]);
								}

								VersionDone = true;

								if (VersionDone)
									Request.Done();

								return Task.CompletedTask;

							}, null);
						}
						else
							VersionDone = true;
					}
					else
						VersionDone = true;

					Request.LogFields(Fields);

					if (VersionDone)
						Request.Done();
				}
				else
					Request.Fail("Unable to perform a service discovery.");

				return Task.CompletedTask;

			}, null);

			return Request;
		}

		public void RegisterComponent(XmppComponent Component)
		{
			lock (this.components)
			{
				if (!this.components.Contains(Component))
					this.components.AddLast(Component);
			}
		}

		public bool UnregisterComponent(XmppComponent Component)
		{
			lock (this.components)
			{
				return this.components.Remove(Component);
			}
		}

		public void AddContexMenuItems(TreeNode Node, ref string CurrentGroup, ContextMenu Menu)
		{
			LinkedList<IMenuAggregator> Aggregators = null;
			MenuItem Item;

			if (Node == this)
			{
				GroupSeparator(ref CurrentGroup, "Connection", Menu);

				Menu.Items.Add(Item = new MenuItem()
				{
					Header = "_Change password...",
					IsEnabled = (this.client is not null && this.client.State == XmppState.Connected)
				});

				Item.Click += this.ChangePassword_Click;
			}

			lock (this.components)
			{
				foreach (XmppComponent Component in this.components)
				{
					if (Component is IMenuAggregator MenuAggregator)
					{
						Aggregators ??= [];
						Aggregators.AddLast(MenuAggregator);
					}
				}
			}

			if (Aggregators is not null)
			{
				foreach (IMenuAggregator Aggregator in Aggregators)
					Aggregator.AddContexMenuItems(Node, ref CurrentGroup, Menu);
			}
		}

		private void ChangePassword_Click(object Sender, RoutedEventArgs e)
		{
			ChangePasswordForm Dialog = new();
			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				this.client.ChangePassword(Dialog.Password.Password, (sender2, e2) =>
				{
					if (e2.Ok)
					{
						this.connections.Modified = true;
						this.password = Dialog.Password.Password;
						this.passwordHash = string.Empty;
						this.passwordHashMethod = string.Empty;
						this.client.Reconnect(this.client.UserName, this.password);

						MainWindow.SuccessBox("Password successfully changed.");
					}
					else
						MainWindow.ErrorBox("Unable to change password.");

					return Task.CompletedTask;

				}, null);
			}
		}

		public override void Edit()
		{
			ConnectToForm Dialog = new()
			{
				Owner = MainWindow.currentInstance
			};

			Dialog.XmppServer.Text = this.host;
			Dialog.XmppPort.Text = this.port.ToString();
			Dialog.UrlEndpoint.Text = this.urlBindResource;
			Dialog.ConnectionMethod.SelectedIndex = (int)this.transport;
			Dialog.AccountName.Text = this.account;
			Dialog.Password.Password = this.passwordHash;
			Dialog.RetypePassword.Password = this.passwordHash;
			Dialog.PasswordHash = this.passwordHash;
			Dialog.PasswordHashMethod = this.passwordHashMethod;
			Dialog.TrustServerCertificate.IsChecked = this.trustCertificate;
			Dialog.AllowInsecureAuthentication.IsChecked = this.allowInsecureAuthentication;

			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				this.transport = (TransportMethod)Dialog.ConnectionMethod.SelectedIndex;
				this.host = Dialog.XmppServer.Text;
				this.urlBindResource = Dialog.UrlEndpoint.Text;
				this.account = Dialog.AccountName.Text;
				this.passwordHash = Dialog.PasswordHash;
				this.passwordHashMethod = Dialog.PasswordHashMethod;
				this.trustCertificate = Dialog.TrustServerCertificate.IsChecked.HasValue && Dialog.TrustServerCertificate.IsChecked.Value;
				this.allowInsecureAuthentication = Dialog.AllowInsecureAuthentication.IsChecked.HasValue && Dialog.AllowInsecureAuthentication.IsChecked.Value;

				if (!int.TryParse(Dialog.XmppPort.Text, out this.port))
					this.port = XmppCredentials.DefaultPort;

				this.OnUpdated();
			}
		}

		private Task PepClient_SensorData(object _, PersonalEventNotificationEventArgs e)
		{
			if (e.PersonalEvent is SensorData SensorData &&
				SensorData.Fields is not null &&
				this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				List<Waher.Things.DisplayableParameters.Parameter> Parameters = [];

				foreach (Waher.Things.SensorData.Field F in SensorData.Fields)
				{
					if (F is Int32Field I32)
						Parameters.Add(new Int32Parameter(F.Name, F.Name, I32.Value));
					else if (F is Int64Field I64)
						Parameters.Add(new Int64Parameter(F.Name, F.Name, I64.Value));
					else
						Parameters.Add(new Waher.Things.DisplayableParameters.
							StringParameter(F.Name, F.Name, F.ValueString));
				}

				Node.Add([.. Parameters]);
				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserTune(object _, UserTuneEventArgs e)
		{
			if (this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				Node.Add(
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Artist", "Artist", e.Tune.Artist),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Length", "Length", e.Tune.Length?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Rating", "Rating", e.Tune.Rating?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Source", "Source", e.Tune.Source),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Title", "Title", e.Tune.Title),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_Track", "Track", e.Tune.Track),
					new Waher.Things.DisplayableParameters.StringParameter("Tune_URI", "URI", e.Tune.Uri?.ToString() ?? string.Empty));

				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserMood(object _, UserMoodEventArgs e)
		{
			if (this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				Node.Add(
					new Waher.Things.DisplayableParameters.StringParameter("Mood_Mood", "Mood", e.Mood.Mood?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Mood_Text", "Text", e.Mood.Text));

				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserLocation(object _, UserLocationEventArgs e)
		{
			if (this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				Node.Add(
					new Waher.Things.DisplayableParameters.StringParameter("Location_Artist", "Accuracy", e.Location.Accuracy?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Alt", "Alt", e.Location.Alt?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_AltAccuracy", "AltAccuracy", e.Location.AltAccuracy?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Area", "Area", e.Location.Area ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Bearing", "Bearing", e.Location.Bearing?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Building", "Building", e.Location.Building ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Country", "Country", e.Location.Country ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_CountryCode", "CountryCode", e.Location.CountryCode ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Datum", "Datum", e.Location.Datum ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Description", "Description", e.Location.Description ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Floor", "Floor", e.Location.Floor ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Lat", "Lat", e.Location.Lat?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Lon", "Lon", e.Location.Lon?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Locality", "Locality", e.Location.Locality ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_PostalCode", "PostalCode", e.Location.PostalCode ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Region", "Region", e.Location.Region ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Room", "Room", e.Location.Room ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Speed", "Speed", e.Location.Speed?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Street", "Street", e.Location.Street ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Text", "Text", e.Location.Text ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_Timestamp", "Timestamp", e.Location.Timestamp?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_TimeZone", "TimeZone", e.Location.TimeZone ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Location_URI", "URI", e.Location.Uri?.ToString() ?? string.Empty));

				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserActivity(object _, UserActivityEventArgs e)
		{
			if (this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				Node.Add(
					new Waher.Things.DisplayableParameters.StringParameter("Activity_General", "General", e.Activity.GeneralActivity?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Activity_Specific", "Specific", e.Activity.SpecificActivity?.ToString() ?? string.Empty),
					new Waher.Things.DisplayableParameters.StringParameter("Activity_Text", "Text", e.Activity.Text));

				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserAvatarMetaData(object _, UserAvatarMetaDataEventArgs e)
		{
			// TODO: Avatars
			return Task.CompletedTask;
		}

		private void ServerlessMessaging_OnNewXmppClient(object _, PeerConnectionEventArgs e)
		{
			XmppClient Client = e.Client;

			this.e2eEncryption.RegisterHandlers(Client);
		}

		private void ServerlessMessaging_OnResynch(object _, ResynchEventArgs e)
		{
			if (this.e2eEncryption is null)
				e.Done(false);
			else
			{
				this.e2eEncryption.SynchronizeE2e(e.RemoteFullJid, (Sender2, e2) =>
				{
					return e.Done(e2.Ok);
				});
			}
		}

		internal void ReregisterView(string SessionId, RemoteDesktopView View)
		{
			lock (this.activeViews)
			{
				this.activeViews[SessionId] = View;
			}
		}

		internal void UnregisterView(RemoteDesktopView View)
		{
			lock (this.activeViews)
			{
				this.activeViews.Remove(View.Session.SessionId);
			}
		}

		private Task Proxy_OnOpen(object _, ValidateStreamEventArgs e)
		{
			RemoteDesktopView View;

			lock (this.activeViews)
			{
				if (!this.activeViews.TryGetValue(e.StreamId, out View))
					return Task.CompletedTask;
			}

			e.AcceptStream(View.Socks5DataReceived, View.Socks5StreamClosed, null);

			return Task.CompletedTask;
		}

		/// <summary>
		/// If node can be copied to clipboard.
		/// </summary>
		public override bool CanCopy => this.client is not null;

		/// <summary>
		/// Is called when the user wants to copy the node to the clipboard.
		/// </summary>
		public override void Copy()
		{
			Clipboard.SetText("xmpp:" + this.client.BareJID);
		}

		/// <summary>
		/// If node can be pasted to, from the clipboard.
		/// </summary>
		public override bool CanPaste
		{
			get
			{
				return this.CanPasteFromClipboard(out _);
			}
		}

		private bool CanPasteFromClipboard(out string BareJid)
		{
			BareJid = null;

			if (this.client is null || this.client.State != XmppState.Connected)
				return false;

			if (!Clipboard.ContainsText(TextDataFormat.Text))
				return false;

			string s = Clipboard.GetText(TextDataFormat.Text);
			if (!s.StartsWith("xmpp:"))
				return false;

			s = s[5..];
			if (!XmppClient.BareJidRegEx.IsMatch(s))
				return false;

			BareJid = s;
			return true;
		}

		/// <summary>
		/// Is called when the user wants to paste data from the clipboard to the node.
		/// </summary>
		public override void Paste()
		{
			if (this.CanPasteFromClipboard(out string BareJid))
				this.client.RequestPresenceSubscription(BareJid);
		}
	}
}
