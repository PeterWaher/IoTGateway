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
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Client.WPF.Dialogs;
using System.Windows.Controls;

namespace Waher.Client.WPF.Model
{
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

		private LinkedList<KeyValuePair<DateTime, MessageEventArgs>> unhandledMessages = new LinkedList<KeyValuePair<DateTime, MessageEventArgs>>();
		private LinkedList<XmppComponent> components = new LinkedList<XmppComponent>();
		private Dictionary<string, List<RosterItemEventHandler>> rosterSubscriptions = new Dictionary<string, List<RosterItemEventHandler>>(StringComparer.CurrentCultureIgnoreCase);
		private Connections connections;
		private XmppClient client;
		private SensorClient sensorClient;
		private ControlClient controlClient;
		private ConcentratorClient concentratorClient;
		private Timer connectionTimer;
		private Exception lastError = null;
		private string host;
		private string domain;
		private int port;
		private string account;
		private string password;
		private string passwordHash;
		private string passwordHashMethod;
		private bool trustCertificate;
		private bool connected = false;
		private bool supportsSearch = false;
		private bool allowInsecureAuthentication = false;
		private bool supportsHashes = true;

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
		/// <param name="AllowInsecureAuthentication">If insecure authentication mechanisms are to be allowed.</param>
		public XmppAccountNode(Connections Connections, TreeNode Parent, string Host, int Port, string Account,
			string PasswordHash, string PasswordHashMethod, bool TrustCertificate, bool AllowInsecureAuthentication)
			: base(Parent)
		{
			this.connections = Connections;
			this.host = this.domain = Host;
			this.port = Port;
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
			this.domain = XML.Attribute(E, "domain", this.host);
			this.port = XML.Attribute(E, "port", 5222);
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
			this.client.OnNormalMessage += Client_OnNormalMessage;

			if (this.allowInsecureAuthentication)
			{
				this.client.AllowPlain = true;
				this.client.AllowCramMD5 = true;
				this.client.AllowDigestMD5 = true;
			}

			this.client.SetPresence(Availability.Chat);

			this.sensorClient = new SensorClient(this.client);
			this.controlClient = new ControlClient(this.client);
			this.concentratorClient = new ConcentratorClient(this.client);

			this.client.Connect();
		}

		private void Client_OnNormalMessage(object Sender, MessageEventArgs e)
		{
			DateTime Now = DateTime.Now;
			DateTime Limit = Now.AddMinutes(-1);

			lock (this.unhandledMessages)
			{
				this.unhandledMessages.AddLast(new KeyValuePair<DateTime, MessageEventArgs>(Now, e));

				while (this.unhandledMessages.First != null && this.unhandledMessages.First.Value.Key <= Limit)
					this.unhandledMessages.RemoveFirst();
			}
		}

		public IEnumerable<MessageEventArgs> GetUnhandledMessages(string LocalName, string Namespace)
		{
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
						yield return Loop.Value.Value;
						this.unhandledMessages.Remove(Loop);
					}

					Loop = Next;
				}
			}
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
		}

		public string Host { get { return this.host; } }
		public int Port { get { return this.port; } }
		public string Account { get { return this.account; } }
		public string PasswordHash { get { return this.passwordHash; } }
		public string PasswordHashMethod { get { return this.passwordHashMethod; } }
		public bool TrustCertificate { get { return this.trustCertificate; } }
		public bool AllowInsecureAuthentication { get { return this.allowInsecureAuthentication; } }
		public bool SupportsHashes { get { return this.supportsHashes; } }

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
			Output.WriteAttributeString("domain", this.domain);
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
		internal static readonly BitmapImage folderClosed = new BitmapImage(new Uri("../Graphics/folder-yellow-icon.png", UriKind.Relative));
		internal static readonly BitmapImage folderOpen = new BitmapImage(new Uri("../Graphics/folder-yellow-open-icon.png", UriKind.Relative));
		internal static readonly BitmapImage box = new BitmapImage(new Uri("../Graphics/App-miscellaneous-icon.png", UriKind.Relative));
		internal static readonly BitmapImage hourglass = new BitmapImage(new Uri("../Graphics/hourglass-icon.png", UriKind.Relative));
		internal static readonly BitmapImage database = new BitmapImage(new Uri("../Graphics/Database-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage component = new BitmapImage(new Uri("../Graphics/server-components-icon_16.png", UriKind.Relative));
		internal static readonly BitmapImage none = new BitmapImage(new Uri("../Graphics/None.png", UriKind.Relative));
		internal static readonly BitmapImage from = new BitmapImage(new Uri("../Graphics/From.png", UriKind.Relative));
		internal static readonly BitmapImage to = new BitmapImage(new Uri("../Graphics/To.png", UriKind.Relative));
		internal static readonly BitmapImage both = new BitmapImage(new Uri("../Graphics/Both.png", UriKind.Relative));

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

		public override bool CanEdit => false;		// TODO: Edit connection properties.
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

			if (Contacts == null)
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
				else
				{
					foreach (KeyValuePair<string, TreeNode> P in this.children)
					{
						if (P.Value is XmppContact Contact &&
							!Existing.ContainsKey(Contact.BareJID))
						{
							if (Removed == null)
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

				this.CheckRosterItemSubscriptions(Item);
			}
		}

		private void CheckRosterItemSubscriptions(RosterItem Item)
		{
			RosterItemEventHandler[] h;

			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(Item.BareJid, out List<RosterItemEventHandler> List))
					h = List.ToArray();
				else
					h = null;
			}

			if (h != null)
			{
				foreach (RosterItemEventHandler h2 in h)
				{
					try
					{
						h2(this, Item);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		public void RegisterRosterEventHandler(string BareJid, RosterItemEventHandler Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (!this.rosterSubscriptions.TryGetValue(BareJid, out List<RosterItemEventHandler> h))
				{
					h = new List<RosterItemEventHandler>();
					this.rosterSubscriptions[BareJid] = h;
				}

				h.Add(Callback);
			}
		}

		public void UnregisterRosterEventHandler(string BareJid, RosterItemEventHandler Callback)
		{
			lock (this.rosterSubscriptions)
			{
				if (this.rosterSubscriptions.TryGetValue(BareJid, out List<RosterItemEventHandler> h) && h.Remove(Callback) && h.Count == 0)
					this.rosterSubscriptions.Remove(BareJid);
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

				RosterItem Item = this.client[e.FromBareJID];
				if (Item != null)
					this.CheckRosterItemSubscriptions(Item);
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
			get { return this.client != null; }
		}

		public override void Recycle(MainWindow Window)
		{
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
					}, null, null);
				}
			}, null);

			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, (sender, e) =>
			{
				foreach (Item Item in e.Items)
				{
					this.client.SendServiceDiscoveryRequest(Item.JID, (sender2, e2) =>
					{
						try
						{
							XmppComponent Component = null;
							ThingRegistry ThingRegistry = null;

							if (this.children == null)
								this.children = new SortedDictionary<string, TreeNode>();

							lock (this.children)
							{
								if (!this.children.ContainsKey(Item.JID))
								{
									if (e2.HasFeature(ThingRegistryClient.NamespaceDiscovery))
									{
										ThingRegistry = new ThingRegistry(this, Item.JID, Item.Name, Item.Node, e2.Features);
										Component = ThingRegistry;
									}
									else if (e2.HasFeature(EventLog.NamespaceEventLogging))
										Component = new EventLog(this, Item.JID, Item.Name, Item.Node, e2.Features);
									else
										Component = new XmppComponent(this, Item.JID, Item.Name, Item.Node, e2.Features);

									this.children[Item.JID] = Component;
								}
							}

							if (Component != null)
								this.connections.Owner.MainView.NodeAdded(this, Component);

							if (ThingRegistry != null && ThingRegistry.SupportsProvisioning)
							{
								MainWindow.currentInstance.Dispatcher.BeginInvoke(new ThreadStart(() =>
									MainWindow.currentInstance.NewQuestion(this, ThingRegistry.ProvisioningClient, null)));
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

					}, null);
				}
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
			public override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
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
			public override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
			{
				string Password = Strings[0];

				if (Password != Field.Form["Password"].ValueString)
					Field.Error = "Passwords don't match.";
			}
		}

		private void ChangePassword(object Sender, DataForm Form)
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

			}, null);
		}

		private void CancelChangePassword(object Sender, DataForm Form)
		{
			// Do nothing.
		}

		public override bool CanReadSensorData => this.IsOnline;

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			return this.DoReadout(Things.SensorData.FieldType.All);
		}

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			return this.DoReadout(Things.SensorData.FieldType.Momentary);
		}

		private SensorDataClientRequest DoReadout(Things.SensorData.FieldType Types)
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
					List<Things.SensorData.Field> Fields = new List<Things.SensorData.Field>();
					DateTime Now = DateTime.Now;

					foreach (KeyValuePair<string, bool> Feature in e.Features)
					{
						Fields.Add(new Things.SensorData.BooleanField(Things.ThingReference.Empty, Now,
							Feature.Key, Feature.Value, Things.SensorData.FieldType.Momentary, Things.SensorData.FieldQoS.AutomaticReadout));
					}

					bool VersionDone = false;

					if ((Types & Things.SensorData.FieldType.Identity) != 0)
					{
						foreach (Identity Identity in e.Identities)
						{
							Fields.Add(new Things.SensorData.StringField(Things.ThingReference.Empty, Now,
								Identity.Type, Identity.Category + (string.IsNullOrEmpty(Identity.Name) ? string.Empty : " (" + Identity.Name + ")"),
								Things.SensorData.FieldType.Identity,
								Things.SensorData.FieldQoS.AutomaticReadout));
						}

						if (e.HasFeature(XmppClient.NamespaceSoftwareVersion))
						{
							this.client.SendSoftwareVersionRequest(string.Empty, (sender2, e2) =>
							{
								Now = DateTime.Now;

								if (e2.Ok)
								{
									Request.LogFields(new Things.SensorData.Field[]
									{
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Server, Name", e2.Name,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Server, OS", e2.OS,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
										new Things.SensorData.StringField(Things.ThingReference.Empty, Now, "Server, Version", e2.Version,
											Things.SensorData.FieldType.Identity, Things.SensorData.FieldQoS.AutomaticReadout),
									});
								}
								else
								{
									Request.LogErrors(new Things.ThingError[]
									{
										new Things.ThingError(Things.ThingReference.Empty, Now, "Unable to read software version.")
									});
								}

								VersionDone = true;

								if (VersionDone)
									Request.Done();

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
			}, null);

			return Request;
		}

		public void RegisterComponent(XmppComponent Component)
		{
			if (!this.components.Contains(Component))
				this.components.AddLast(Component);
		}

		public void UnregisterComponent(XmppComponent Component)
		{
			this.components.Remove(Component);
		}

		public void AddContexMenuItems(TreeNode Node, ref string CurrentGroup, ContextMenu Menu)
		{
			foreach (XmppComponent Component in this.components)
			{
				if (Component is IMenuAggregator MenuAggregator)
					MenuAggregator.AddContexMenuItems(Node, ref CurrentGroup, Menu);
			}
		}

	}
}
