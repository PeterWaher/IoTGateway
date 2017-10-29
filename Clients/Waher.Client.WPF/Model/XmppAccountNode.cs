using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Events;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Class representing a normal XMPP account.
	/// </summary>
	public class XmppAccountNode : TreeNode
	{
		private const string SensorGroupName = "Sensors";
		private const string EventsGroupName = "Events";
		private const string ActuatorGroupName = "Actuators";
		private const string ConcentratorGroupName = "Concentrators";
		private const string OtherGroupName = "Others";

		private Connections connections;
		private XmppClient client;
		private SensorClient sensorClient;
		private ControlClient controlClient;
		private ConcentratorClient concentratorClient;
		private Timer connectionTimer;
		private Exception lastError = null;
		private string host;
		private int port;
		private string account;
		private string password;
		private string passwordHash;
		private string passwordHashMethod;
		private bool trustCertificate;
		private bool connected = false;

		/// <summary>
		/// Class representing a normal XMPP account.
		/// </summary>
		/// <param name="Connections">Connections object.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Host">Host name.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="Account">Account name.</param>
		/// <param name="PasswordHash">Password hash.</param>
		/// <param name="PasswordHashMethod">Password hash method.</param>
		/// <param name="TrustCertificate">If the server certificate should be trusted.</param>
		public XmppAccountNode(Connections Connections, TreeNode Parent, string Host, int Port, string Account,
			string PasswordHash, string PasswordHashMethod, bool TrustCertificate)
			: base(Parent)
		{
			this.connections = Connections;
			this.host = Host;
			this.port = Port;
			this.account = Account;
			this.password = string.Empty;
			this.passwordHash = PasswordHash;
			this.passwordHashMethod = PasswordHashMethod;
			this.trustCertificate = TrustCertificate;

			this.Init();
		}

		public XmppAccountNode(XmlElement E, Connections Connections, TreeNode Parent)
			: base(Parent)
		{
			this.connections = Connections;
			this.host = XML.Attribute(E, "host");
			this.port = XML.Attribute(E, "port", 5222);
			this.account = XML.Attribute(E, "account");
			this.password = XML.Attribute(E, "password");
			this.passwordHash = XML.Attribute(E, "passwordHash");
			this.passwordHashMethod = XML.Attribute(E, "passwordHashMethod");
			this.trustCertificate = XML.Attribute(E, "trustCertificate", false);

			this.Init();
		}

		private void Init(params ISniffer[] Sniffers)
		{
			if (!string.IsNullOrEmpty(this.passwordHash))
				this.client = new XmppClient(this.host, this.port, this.account, this.passwordHash, this.passwordHashMethod, "en", typeof(App).Assembly);
			else
				this.client = new XmppClient(this.host, this.port, this.account, this.password, "en", typeof(App).Assembly);

			if (Sniffers != null)
				this.client.AddRange(Sniffers);

			this.client.TrustServer = this.trustCertificate;
			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.client.OnError += new XmppExceptionEventHandler(Client_OnError);
			this.client.OnPresence += new PresenceEventHandler(Client_OnPresence);
			this.client.OnPresenceSubscribe += new PresenceEventHandler(Client_OnPresenceSubscribe);
			this.client.OnPresenceUnsubscribe += new PresenceEventHandler(Client_OnPresenceUnsubscribe);
			this.client.OnRosterItemAdded += new RosterItemEventHandler(Client_OnRosterItemUpdated);
			this.client.OnRosterItemRemoved += new RosterItemEventHandler(Client_OnRosterItemRemoved);
			this.client.OnRosterItemUpdated += new RosterItemEventHandler(Client_OnRosterItemUpdated);
			this.connectionTimer = new Timer(this.CheckConnection, null, 60000, 60000);

			this.client.SetPresence(Availability.Chat);

			this.sensorClient = new SensorClient(this.client);
			this.controlClient = new ControlClient(this.client);
			this.concentratorClient = new ConcentratorClient(this.client);

			this.client.Connect();
		}

		private void Client_OnError(object Sender, Exception Exception)
		{
			this.lastError = Exception;
		}

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected = true;
					this.lastError = null;

					if (string.IsNullOrEmpty(this.passwordHash))
					{
						this.passwordHash = this.client.PasswordHash;
						this.passwordHashMethod = this.client.PasswordHashMethod;
					}

					this.CheckRoster();
					break;

				case XmppState.Offline:
					bool ImmediateReconnect = this.connected;
					this.connected = false;

					if (ImmediateReconnect && this.client != null)
						this.client.Reconnect();
					break;
			}

			this.OnUpdated();
		}

		public string Host { get { return this.host; } }
		public int Port { get { return this.port; } }
		public string Account { get { return this.account; } }
		public string PasswordHash { get { return this.passwordHash; } }
		public string PasswordHashMethod { get { return this.passwordHashMethod; } }
		public bool TrustCertificate { get { return this.trustCertificate; } }

		public override string Header
		{
			get { return this.account + "@" + this.host; }
		}

		public override string TypeName
		{
			get { return "XMPP Account"; }
		}

		public override void Dispose()
		{
			base.Dispose();

			if (this.connectionTimer != null)
			{
				this.connectionTimer.Dispose();
				this.connectionTimer = null;
			}

			if (this.sensorClient != null)
			{
				this.sensorClient.Dispose();
				this.sensorClient = null;
			}

			if (this.controlClient != null)
			{
				this.controlClient.Dispose();
				this.controlClient = null;
			}

			if (this.concentratorClient != null)
			{
				this.concentratorClient.Dispose();
				this.concentratorClient = null;
			}

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
			Output.WriteAttributeString("port", this.port.ToString());
			Output.WriteAttributeString("account", this.account);

			if (string.IsNullOrEmpty(this.passwordHash))
				Output.WriteAttributeString("password", this.password);
			else
			{
				Output.WriteAttributeString("passwordHash", this.passwordHash);
				Output.WriteAttributeString("passwordHashMethod", this.passwordHashMethod);
			}

			Output.WriteAttributeString("trustCertificate", CommonTypes.Encode(this.trustCertificate));
			Output.WriteEndElement();
		}

		internal static readonly BitmapImage away = new BitmapImage(new Uri("../Graphics/Away.png", UriKind.Relative));
		internal static readonly BitmapImage busy = new BitmapImage(new Uri("../Graphics/DoNotDisturb.png", UriKind.Relative));
		internal static readonly BitmapImage chat = new BitmapImage(new Uri("../Graphics/Chat.png", UriKind.Relative));
		internal static readonly BitmapImage extendedAway = new BitmapImage(new Uri("../Graphics/ExtendedAway.png", UriKind.Relative));
		internal static readonly BitmapImage offline = new BitmapImage(new Uri("../Graphics/Offline.png", UriKind.Relative));
		internal static readonly BitmapImage online = new BitmapImage(new Uri("../Graphics/Online.png", UriKind.Relative));
		internal static readonly BitmapImage folderClosed = new BitmapImage(new Uri("../Graphics/folder-yellow-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderOpen = new BitmapImage(new Uri("../Graphics/folder-yellow-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage box = new BitmapImage(new Uri("../Graphics/App-miscellaneous-icon.png", UriKind.Relative));
		internal static readonly BitmapImage hourglass = new BitmapImage(new Uri("../Graphics/hourglass-icon.png", UriKind.Relative));
		
		public override ImageSource ImageResource
		{
			get
			{
				if (this.client == null)
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
						if (this.lastError == null)
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
			LinkedList<TreeNode> Added = null;
			LinkedList<RosterItem> Resubscribe = null;
			LinkedList<RosterItem> Reunsubscribe = null;

			if (Contacts == null)
				Contacts = new SortedDictionary<string, TreeNode>();

			lock (Contacts)
			{
				XmppContact Contact;

				foreach (RosterItem Item in this.client.Roster)
				{
					if (!Contacts.ContainsKey(Item.BareJid))
					{
						if (Item.IsInGroup(ConcentratorGroupName))
							Contact = new XmppConcentrator(this, this.client, Item.BareJid, Item.IsInGroup(EventsGroupName));
						else if (Item.IsInGroup(ActuatorGroupName))
							Contact = new XmppActuator(this, this.client, Item.BareJid, Item.IsInGroup(SensorGroupName), Item.IsInGroup(EventsGroupName));
						else if (Item.IsInGroup(SensorGroupName))
							Contact = new XmppSensor(this, this.client, Item.BareJid, Item.IsInGroup(EventsGroupName));
						else if (Item.IsInGroup(OtherGroupName))
							Contact = new XmppOther(this, this.client, Item.BareJid);
						else
							Contact = new XmppContact(this, this.client, Item.BareJid);

						Contacts[Item.BareJid] = Contact;

						if (Added == null)
							Added = new LinkedList<TreeNode>();

						Added.AddLast(Contact);
					}

					switch (Item.PendingSubscription)
					{
						case PendingSubscription.Subscribe:
							if (Resubscribe == null)
								Resubscribe = new LinkedList<RosterItem>();

							Resubscribe.AddLast(Item);
							break;

						case PendingSubscription.Unsubscribe:
							if (Reunsubscribe == null)
								Reunsubscribe = new LinkedList<RosterItem>();

							Reunsubscribe.AddLast(Item);
							break;
					}
				}

				if (this.children == null)
					this.children = Contacts;
			}

			if (Added != null)
			{
				foreach (TreeNode Node in Added)
					this.connections.Owner.MainView.NodeAdded(this, Node);
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

		private void Client_OnRosterItemUpdated(object Sender, RosterItem Item)
		{
			if (this.children == null)
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
						Contact = new XmppContact(this, this.client, Item.BareJid);
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
			}
		}

		private void Client_OnRosterItemRemoved(object Sender, RosterItem Item)
		{
			if (this.children == null)
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
		}

		private void Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			if (this.children == null)
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
						this.client.SendServiceDiscoveryRequest(e.From, this.ServiceDiscoveryResponse, Node);
				}
				else if (e.FromBareJID == this.client.BareJID)
					this.client.Information("Presence from same bare JID. Ignored.");
				else
					this.client.Error("Presence from node not found in roster: " + e.FromBareJID);
			}
		}

		private void ServiceDiscoveryResponse(object Sender, ServiceDiscoveryEventArgs e)
		{
			if (e.Ok)
			{
				XmppContact Node = (XmppContact)e.State;
				object OldTag;

				if (e.HasFeature(ConcentratorServer.NamespaceConcentrator))
				{
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);

					OldTag = Node.Tag;
					Node = new XmppConcentrator(Node.Parent, this.client, Node.BareJID, SupportsEvents)
                    {
					    Tag = OldTag
                    };

					this.children[Node.Key] = Node;

					if (SupportsEvents)
						this.AddGroups(Node, ConcentratorGroupName, EventsGroupName);
					else
						this.AddGroups(Node, ConcentratorGroupName);
				}
				else if (e.HasFeature(ControlClient.NamespaceControl))
				{
					bool IsSensor = e.HasFeature(SensorClient.NamespaceSensorData);
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);

					OldTag = Node.Tag;
					Node = new XmppActuator(Node.Parent, this.client, Node.BareJID, IsSensor, SupportsEvents)
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

					this.AddGroups(Node, Groups.ToArray());
				}
				else if (e.HasFeature(SensorClient.NamespaceSensorData))
				{
					bool SupportsEvents = e.HasFeature(SensorClient.NamespaceSensorEvents);

					OldTag = Node.Tag;
					Node = new XmppSensor(Node.Parent, this.client, Node.BareJID, SupportsEvents)
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

					this.AddGroups(Node, Groups.ToArray());
				}
				else
				{
					OldTag = Node.Tag;
					Node = new XmppOther(Node.Parent, this.client, Node.BareJID)
                    {
					    Tag = OldTag
                    };

					this.children[Node.Key] = Node;

					this.AddGroups(Node, OtherGroupName);
				}

				this.OnUpdated();
			}
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

		private void Client_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			this.connections.Owner.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.PresenceSubscribe), e);
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
					if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
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

		private void Client_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();
		}

		public override bool CanRecycle
		{
			get { return true; }
		}

		public override void Recycle(MainWindow Window)
		{
			ISniffer[] Sniffers = null;

			this.Removed(Window);

			if (this.client != null)
			{
				XmppClient Client = this.client;
				Sniffers = Client.Sniffers;
				this.client = null;
				Client.Dispose();
			}

			this.Init(Sniffers);
			this.Added(Window);
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
				if (this.client == null)
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

		public override bool Delete(TreeNode Node)
		{
			if (base.Delete(Node))
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
			if (this.client == null)
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
	}
}
