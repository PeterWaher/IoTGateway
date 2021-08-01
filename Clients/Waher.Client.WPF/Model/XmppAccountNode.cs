using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Events;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.RDP;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.Synchronization;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model.Concentrator;
using Waher.Client.WPF.Model.Legal;
using Waher.Client.WPF.Model.Muc;
using Waher.Client.WPF.Model.Provisioning;
using Waher.Client.WPF.Model.PubSub;
using Waher.Client.WPF.Model.Things;

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
	public class XmppAccountNode : TreeNode, IMenuAggregator
	{
		private const string SensorGroupName = "Sensors";
		private const string EventsGroupName = "Events";
		private const string ActuatorGroupName = "Actuators";
		private const string ConcentratorGroupName = "Concentrators";
		private const string OtherGroupName = "Others";
		private const string RemoteDesktopGroupName = "RDP";

		private readonly LinkedList<KeyValuePair<DateTime, MessageEventArgs>> unhandledMessages = new LinkedList<KeyValuePair<DateTime, MessageEventArgs>>();
		private readonly LinkedList<XmppComponent> components = new LinkedList<XmppComponent>();
		private readonly Dictionary<string, List<RosterItemEventHandlerAsync>> rosterSubscriptions = new Dictionary<string, List<RosterItemEventHandlerAsync>>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Connections connections;
		private XmppClient client;
		private PepClient pepClient;
		private SensorClient sensorClient;
		private ControlClient controlClient;
		private ConcentratorClient concentratorClient;
		private SynchronizationClient synchronizationClient;
		private MultiUserChatClient mucClient;
		private RemoteDesktopClient rdpClient;
		private Timer connectionTimer;
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
			this.transport = (TransportMethod)XML.Attribute(E, "transport", TransportMethod.TraditionalSocket);
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
			XmppCredentials Credentials = new XmppCredentials()
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

			this.client = new XmppClient(Credentials, "en", typeof(App).Assembly);

			if (Sniffers != null)
				this.client.AddRange(Sniffers);

			this.client.OnStateChanged += this.Client_OnStateChanged;
			this.client.OnError += this.Client_OnError;
			this.client.OnPresence += this.Client_OnPresence;
			this.client.OnPresenceSubscribe += this.Client_OnPresenceSubscribe;
			this.client.OnPresenceUnsubscribe += this.Client_OnPresenceUnsubscribe;
			this.client.OnRosterItemAdded += this.Client_OnRosterItemUpdated;
			this.client.OnRosterItemRemoved += this.Client_OnRosterItemRemoved;
			this.client.OnRosterItemUpdated += this.Client_OnRosterItemUpdated;
			this.connectionTimer = new Timer(this.CheckConnection, null, 60000, 60000);
			this.client.OnNormalMessage += Client_OnNormalMessage;
			this.client.OnErrorMessage += Client_OnErrorMessage;

			this.client.SetPresence(Availability.Chat);

			this.sensorClient = new SensorClient(this.client);
			this.controlClient = new ControlClient(this.client);
			this.concentratorClient = new ConcentratorClient(this.client);
			this.synchronizationClient = new SynchronizationClient(this.client);
			this.rdpClient = new RemoteDesktopClient(this.client);

			this.AddPepClient(string.Empty);

			this.concentratorClient.OnEvent += ConcentratorClient_OnEvent;

			this.client.Connect();
		}

		private Task Client_OnErrorMessage(object Sender, MessageEventArgs e)
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

			this.pepClient.OnUserActivity += PepClient_OnUserActivity;
			this.pepClient.OnUserAvatarMetaData += PepClient_OnUserAvatarMetaData;
			this.pepClient.OnUserLocation += PepClient_OnUserLocation;
			this.pepClient.OnUserMood += PepClient_OnUserMood;
			this.pepClient.OnUserTune += PepClient_OnUserTune;
			this.pepClient.RegisterHandler(typeof(SensorData), PepClient_SensorData);
		}

		private void AddMucClient(string MucComponentAddress)
		{
			this.mucClient?.Dispose();
			this.mucClient = null;

			this.mucClient = new MultiUserChatClient(this.client, MucComponentAddress);
		}

		private async Task ConcentratorClient_OnEvent(object Sender, SourceEventMessageEventArgs EventMessage)
		{
			if (this.TryGetChild(EventMessage.FromBareJID, out TreeNode Child) &&
				(Child is XmppConcentrator Concentrator))
			{
				await Concentrator.ConcentratorClient_OnEvent(Sender, EventMessage);
			}
		}

		private Task Client_OnNormalMessage(object Sender, MessageEventArgs e)
		{
			DateTime Now = DateTime.Now;
			DateTime Limit = Now.AddMinutes(-1);

			lock (this.unhandledMessages)
			{
				this.unhandledMessages.AddLast(new KeyValuePair<DateTime, MessageEventArgs>(Now, e));

				while (this.unhandledMessages.First != null && this.unhandledMessages.First.Value.Key <= Limit)
					this.unhandledMessages.RemoveFirst();
			}

			return Task.CompletedTask;
		}

		public IEnumerable<MessageEventArgs> GetUnhandledMessages(string LocalName, string Namespace)
		{
			LinkedList<MessageEventArgs> Result = new LinkedList<MessageEventArgs>();
			LinkedListNode<KeyValuePair<DateTime, MessageEventArgs>> Loop, Next;
			bool Found;

			lock (this.unhandledMessages)
			{
				Loop = this.unhandledMessages.First;

				while (Loop != null)
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

		private Task Client_OnStateChanged(object _, XmppState NewState)
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
					break;

				case XmppState.Offline:
					bool ImmediateReconnect = this.connected;
					this.connected = false;

					if (ImmediateReconnect && this.client != null)
						this.client.Reconnect();
					break;
			}

			this.OnUpdated();

			return Task.CompletedTask;
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

			this.connectionTimer?.Dispose();
			this.connectionTimer = null;

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

			if (this.client != null)
			{
				XmppClient Client = this.client;
				this.client = null;
				Client.Dispose();
			}
		}

		private void CheckConnection(object P)
		{
			if (this.client != null && (this.client.State == XmppState.Offline || this.client.State == XmppState.Error || this.client.State == XmppState.Authenticating))
			{
				try
				{
					this.client.Reconnect();
				}
				catch (Exception ex)
				{
					MessageBox.Show(MainWindow.currentInstance, ex.Message, "Unable to reconnect.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
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

		internal static readonly BitmapImage away = new BitmapImage(new Uri("../Graphics/Away.png", UriKind.Relative));
		internal static readonly BitmapImage busy = new BitmapImage(new Uri("../Graphics/DoNotDisturb.png", UriKind.Relative));
		internal static readonly BitmapImage chat = new BitmapImage(new Uri("../Graphics/Chat.png", UriKind.Relative));
		internal static readonly BitmapImage extendedAway = new BitmapImage(new Uri("../Graphics/ExtendedAway.png", UriKind.Relative));
		internal static readonly BitmapImage offline = new BitmapImage(new Uri("../Graphics/Offline.png", UriKind.Relative));
		internal static readonly BitmapImage online = new BitmapImage(new Uri("../Graphics/Online.png", UriKind.Relative));
		internal static readonly BitmapImage folderBlueClosed = new BitmapImage(new Uri("../Graphics/folder-blue-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderBlueOpen = new BitmapImage(new Uri("../Graphics/folder-blue-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderYellowClosed = new BitmapImage(new Uri("../Graphics/folder-yellow-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderYellowOpen = new BitmapImage(new Uri("../Graphics/folder-yellow-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage box = new BitmapImage(new Uri("../Graphics/App-miscellaneous-icon.png", UriKind.Relative));
		internal static readonly BitmapImage hourglass = new BitmapImage(new Uri("../Graphics/hourglass-icon.png", UriKind.Relative));
		internal static readonly BitmapImage database = new BitmapImage(new Uri("../Graphics/Database-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage component = new BitmapImage(new Uri("../Graphics/server-components-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage chatBubble = new BitmapImage(new Uri("../Graphics/Chat-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage legal = new BitmapImage(new Uri("../Graphics/justice-balance-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage log = new BitmapImage(new Uri("../Graphics/App-edit-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage none = new BitmapImage(new Uri("../Graphics/None.png", UriKind.Relative));
		internal static readonly BitmapImage from = new BitmapImage(new Uri("../Graphics/From.png", UriKind.Relative));
		internal static readonly BitmapImage to = new BitmapImage(new Uri("../Graphics/To.png", UriKind.Relative));
		internal static readonly BitmapImage both = new BitmapImage(new Uri("../Graphics/Both.png", UriKind.Relative));

		public override ImageSource ImageResource
		{
			get
			{
				if (this.client is null)
					return offline;
				else
				{
					switch (this.client.State)
					{
						case XmppState.Connected:
							return online;

						case XmppState.Error:
							return busy;

						case XmppState.Offline:
						default:
							return offline;
					}
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
				return this.client != null && this.client.State == XmppState.Connected;
			}
		}

		public override bool CanEdit => true;
		public override bool CanDelete => true;

		public override void Add()
		{
			AddContactForm Dialog = new AddContactForm()
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
			Dictionary<string, TreeNode> Existing = new Dictionary<string, TreeNode>();
			LinkedList<TreeNode> Added = null;
			LinkedList<KeyValuePair<string, TreeNode>> Removed = null;
			LinkedList<RosterItem> Resubscribe = null;
			LinkedList<RosterItem> Reunsubscribe = null;

			if (Contacts is null)
				Contacts = new SortedDictionary<string, TreeNode>();

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

						if (Added is null)
							Added = new LinkedList<TreeNode>();

						Added.AddLast(Contact);
					}

					switch (Item.PendingSubscription)
					{
						case PendingSubscription.Subscribe:
							if (Resubscribe is null)
								Resubscribe = new LinkedList<RosterItem>();

							Resubscribe.AddLast(Item);
							break;

						case PendingSubscription.Unsubscribe:
							if (Reunsubscribe is null)
								Reunsubscribe = new LinkedList<RosterItem>();

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
							if (Removed is null)
								Removed = new LinkedList<KeyValuePair<string, TreeNode>>();

							Removed.AddLast(P);
						}
					}

					if (Removed != null)
					{
						foreach (KeyValuePair<string, TreeNode> P in Removed)
							this.children.Remove(P.Key);
					}
				}
			}

			if (Added != null)
			{
				foreach (TreeNode Node in Added)
					this.connections.Owner.MainView.NodeAdded(this, Node);
			}

			if (Removed != null)
			{
				foreach (KeyValuePair<string, TreeNode> P in Removed)
					this.connections.Owner.MainView.NodeRemoved(this, P.Value);
			}

			if (Resubscribe != null)
			{
				foreach (RosterItem Item in Resubscribe)
					this.client.RequestPresenceSubscription(Item.BareJid);
			}

			if (Reunsubscribe != null)
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

		private async Task Client_OnRosterItemUpdated(object Sender, RosterItem Item)
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
						if ((Contact = Node as XmppContact) != null)
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
			RosterItemEventHandlerAsync[] h;

			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(Item.BareJid, out List<RosterItemEventHandlerAsync> List))
					h = List.ToArray();
				else
					h = null;
			}

			if (h != null)
			{
				foreach (RosterItemEventHandlerAsync h2 in h)
				{
					try
					{
						await h2(this, Item);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		public void RegisterRosterEventHandler(string BareJid, RosterItemEventHandlerAsync Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (!this.rosterSubscriptions.TryGetValue(BareJid, out List<RosterItemEventHandlerAsync> h))
				{
					h = new List<RosterItemEventHandlerAsync>();
					this.rosterSubscriptions[BareJid] = h;
				}

				h.Add(Callback);
			}
		}

		public void UnregisterRosterEventHandler(string BareJid, RosterItemEventHandlerAsync Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(BareJid, out List<RosterItemEventHandlerAsync> h) && h.Remove(Callback) && h.Count == 0)
					this.rosterSubscriptions.Remove(BareJid);
			}
		}

		private Task Client_OnRosterItemRemoved(object Sender, RosterItem Item)
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

		private async Task Client_OnPresence(object Sender, PresenceEventArgs e)
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

				if (Node != null)
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
				if (Item != null)
					await this.CheckRosterItemSubscriptions(Item);
			}
		}

		internal void CheckType(TreeNode Node, string FullJid)
		{
			this.client.SendServiceDiscoveryRequest(FullJid, this.ServiceDiscoveryResponse, Node);
		}

		private Task ServiceDiscoveryResponse(object Sender, ServiceDiscoveryEventArgs e)
		{
			if (e.Ok)
			{
				XmppContact Node = (XmppContact)e.State;
				object OldTag;

				if (e.HasFeature(ConcentratorServer.NamespaceConcentrator))
				{
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppConcentrator(Node.Parent, this.client, Node.BareJID, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups = new List<string>()
					{
						ConcentratorGroupName
					};

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, Groups.ToArray());
				}
				else if (e.HasFeature(ControlClient.NamespaceControl))
				{
					bool IsSensor = e.HasFeature(SensorClient.NamespaceSensorData);
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppActuator(Node.Parent, this.client, Node.BareJID, IsSensor, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups = new List<string>()
					{
						ActuatorGroupName
					};

					if (IsSensor)
						Groups.Add(SensorGroupName);

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, Groups.ToArray());
				}
				else if (e.HasFeature(SensorClient.NamespaceSensorData))
				{
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);
					bool SupportsRdp = e.HasFeature(RemoteDesktopClient.RemoteDesktopNamespace);

					OldTag = Node.Tag;
					Node = new XmppSensor(Node.Parent, this.client, Node.BareJID, SupportsEvents, SupportsRdp)
					{
						Tag = OldTag
					};

					this.children[Node.Key] = Node;

					List<string> Groups = new List<string>()
					{
						SensorGroupName
					};

					if (SupportsEvents)
						Groups.Add(EventsGroupName);

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, Groups.ToArray());
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

					List<string> Groups = new List<string>()
					{
						OtherGroupName
					};

					if (SupportsRdp)
						Groups.Add(RemoteDesktopGroupName);

					this.AddGroups(Node, Groups.ToArray());
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

		private Task Client_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			MainWindow.UpdateGui(this.PresenceSubscribe, e);
			return Task.CompletedTask;
		}

		private void PresenceSubscribe(object P)
		{
			PresenceEventArgs e = (PresenceEventArgs)P;

			switch (MessageBox.Show(this.connections.Owner, e.FromBareJID + " has requested to subscribe to your presence (become your friend). Do you accept?",
				this.client.BareJID, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes))
			{
				case MessageBoxResult.Yes:
					e.Accept();

					RosterItem Item = this.client.GetRosterItem(e.FromBareJID);
					if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
						this.client.RequestPresenceSubscription(e.FromBareJID);

					this.client.SetPresence(Availability.Chat);
					break;

				case MessageBoxResult.No:
					e.Decline();
					break;

				case MessageBoxResult.Cancel:
				default:
					// Do nothing.
					break;
			}
		}

		private Task Client_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();
			return Task.CompletedTask;
		}

		public override bool CanRecycle
		{
			get { return this.client != null; }
		}

		public override void Recycle(MainWindow Window)
		{
			if (!(this.children is null))
			{
				foreach (TreeNode Node in this.children.Values)
				{
					if (Node.CanRecycle)
						Node.Recycle(Window);
				}
			}

			this.client.Reconnect();
		}

		public bool IsOnline
		{
			get
			{
				return this.client != null && this.client.State == XmppState.Connected;
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

		public override string Key
		{
			get
			{
				return this.BareJID;
			}
		}

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

		public override bool IsSniffable
		{
			get
			{
				return this.client != null;
			}
		}

		public override void AddSniffer(Networking.Sniffers.ISniffer Sniffer)
		{
			this.client.Add(Sniffer);
		}

		public override bool RemoveSniffer(ISniffer Sniffer)
		{
			if (this.client is null)
				return false;
			else
				return this.client.Remove(Sniffer);
		}

		public XmppClient Client
		{
			get { return this.client; }
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

		public PepClient PepClient
		{
			get { return this.pepClient; }
		}

		public MultiUserChatClient MucClient
		{
			get { return this.mucClient; }
		}

		public SensorClient SensorClient
		{
			get { return this.sensorClient; }
		}

		public ControlClient ControlClient
		{
			get { return this.controlClient; }
		}

		public ConcentratorClient ConcentratorClient
		{
			get { return this.concentratorClient; }
		}

		public SynchronizationClient SynchronizationClient
		{
			get { return this.synchronizationClient; }
		}

		public RemoteDesktopClient RdpClient
		{
			get { return this.rdpClient; }
		}

		public void SearchComponents()
		{
			this.client.SendServiceDiscoveryRequest(this.client.Domain, (sender, e) =>
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

			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, (sender, e) =>
			{
				foreach (Item Item in e.Items)
				{
					this.client.SendServiceDiscoveryRequest(Item.JID, async (sender2, e2) =>
					{
						try
						{
							XmppComponent Component = null;
							ThingRegistry ThingRegistry = null;

							if (this.children is null)
								this.children = new SortedDictionary<string, TreeNode>();

							lock (this.children)
							{
								if (this.children.ContainsKey(Item.JID))
									return;
							}

							if (e2.HasFeature(ThingRegistryClient.NamespaceDiscovery))
							{
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
							else if (e2.HasFeature(ContractsClient.NamespaceLegalIdentities))
								Component = await LegalService.Create(this, Item.JID, Item.Name, Item.Node, e2.Features);
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

							if (ThingRegistry != null && ThingRegistry.SupportsProvisioning)
							{
								MainWindow.UpdateGui(() =>
									MainWindow.currentInstance.NewQuestion(this, ThingRegistry.ProvisioningClient, null));
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

					}, null);
				}

				return Task.CompletedTask;

			}, null);
		}

		public override bool CanConfigure => this.IsOnline;

		public override void GetConfigurationForm(DataFormResultEventHandler Callback, object State)
		{
			DataForm Form = new DataForm(this.client, this.ChangePassword, this.CancelChangePassword, this.BareJID, this.BareJID,
				new TextPrivateField(null, "Password", "New password:", true, new string[] { string.Empty }, null,
					"Enter new password here.", new StringDataType(), new PasswordValidation(), string.Empty, false, false, false),
				new TextPrivateField(null, "Password2", "Retype password:", true, new string[] { string.Empty }, null,
					"Retype password here.", new StringDataType(), new Password2Validation(), string.Empty, false, false, false))
			{
				Title = "Change password",
				Instructions = new string[] { "Enter the new password you wish to use." }
			};

			Callback(this, new DataFormEventArgs(Form, new IqResultEventArgs(null, string.Empty, this.BareJID, this.BareJID, true, State)));
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

			this.client.ChangePassword(NewPassword, (sender, e) =>
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

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			return this.DoReadout(FieldType.All);
		}

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			return this.DoReadout(FieldType.Momentary);
		}

		private SensorDataClientRequest DoReadout(FieldType Types)
		{
			string Id = Guid.NewGuid().ToString();

			CustomSensorDataClientRequest Request = new CustomSensorDataClientRequest(Id, string.Empty, string.Empty, null,
				Types, null, DateTime.MinValue, DateTime.MaxValue, DateTime.Now, string.Empty, string.Empty, string.Empty);

			Request.Accept(false);
			Request.Started();

			this.client.SendServiceDiscoveryRequest(string.Empty, (sender, e) =>
			{
				if (e.Ok)
				{
					List<Waher.Things.SensorData.Field> Fields = new List<Waher.Things.SensorData.Field>();
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
									Request.LogFields(new Waher.Things.SensorData.Field[]
									{
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, Name", e2.Name,
											FieldType.Identity, FieldQoS.AutomaticReadout),
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, OS", e2.OS,
											FieldType.Identity, FieldQoS.AutomaticReadout),
										new StringField(Waher.Things.ThingReference.Empty, Now, "Server, Version", e2.Version,
											FieldType.Identity, FieldQoS.AutomaticReadout),
									});
								}
								else
								{
									Request.LogErrors(new Waher.Things.ThingError[]
									{
										new Waher.Things.ThingError(Waher.Things.ThingReference.Empty, Now, "Unable to read software version.")
									});
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
				this.GroupSeparator(ref CurrentGroup, "Connection", Menu);

				Menu.Items.Add(Item = new MenuItem()
				{
					Header = "_Change password...",
					IsEnabled = (this.client != null && this.client.State == XmppState.Connected)
				});

				Item.Click += this.ChangePassword_Click;
			}

			lock (this.components)
			{
				foreach (XmppComponent Component in this.components)
				{
					if (Component is IMenuAggregator MenuAggregator)
					{
						if (Aggregators is null)
							Aggregators = new LinkedList<IMenuAggregator>();

						Aggregators.AddLast(MenuAggregator);
					}
				}
			}

			if (!(Aggregators is null))
			{
				foreach (IMenuAggregator Aggregator in Aggregators)
					Aggregator.AddContexMenuItems(Node, ref CurrentGroup, Menu);
			}
		}

		private void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
			ChangePasswordForm Dialog = new ChangePasswordForm();
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
			ConnectToForm Dialog = new ConnectToForm()
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

		private Task PepClient_SensorData(object Sender, PersonalEventNotificationEventArgs e)
		{
			if (e.PersonalEvent is SensorData SensorData &&
				SensorData.Fields != null &&
				this.TryGetChild(e.FromBareJID, out TreeNode Node))
			{
				List<Waher.Things.DisplayableParameters.Parameter> Parameters = new List<Waher.Things.DisplayableParameters.Parameter>();

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

				Node.Add(Parameters.ToArray());
				Node.OnUpdated();
			}

			return Task.CompletedTask;
		}

		private Task PepClient_OnUserTune(object Sender, UserTuneEventArguments e)
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

		private Task PepClient_OnUserMood(object Sender, UserMoodEventArguments e)
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

		private Task PepClient_OnUserLocation(object Sender, UserLocationEventArguments e)
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

		private Task PepClient_OnUserActivity(object Sender, UserActivityEventArguments e)
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

		private Task PepClient_OnUserAvatarMetaData(object Sender, UserAvatarMetaDataEventArguments e)
		{
			// TODO: Avatars
			return Task.CompletedTask;
		}

	}
}
