using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.PubSub.Events;
using Waher.Networking.XMPP.ResultSetManagement;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Client managing communication with a Publish/Subscribe component.
	/// https://xmpp.org/extensions/xep-0060.html
	/// </summary>
	public class PubSubClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/pubsub
		/// </summary>
		public const string NamespacePubSub = "http://jabber.org/protocol/pubsub";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#owner
		/// </summary>
		public const string NamespacePubSubOwner = "http://jabber.org/protocol/pubsub#owner";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#event
		/// </summary>
		public const string NamespacePubSubEvents = "http://jabber.org/protocol/pubsub#event";

		/// <summary>
		/// http://jabber.org/protocol/shim
		/// </summary>
		public const string NamespaceStanzaHeaders = "http://jabber.org/protocol/shim";

		/// <summary>
		/// urn:xmpp:delay (XEP-0203)
		/// </summary>
		public const string NamespaceDelayedDelivery = "urn:xmpp:delay";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#node_config
		/// </summary>
		public const string FormTypeNodeConfig = "http://jabber.org/protocol/pubsub#node_config";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#subscribe_options
		/// </summary>
		public const string FormTypeSubscribeOptions = "http://jabber.org/protocol/pubsub#subscribe_options";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#subscribe_authorization
		/// </summary>
		public const string FormTypeSubscriptionAuthorization = "http://jabber.org/protocol/pubsub#subscribe_authorization";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#meta-data
		/// </summary>
		public const string FormTypeNodeMetaData = "http://jabber.org/protocol/pubsub#meta-data";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#access-authorize
		/// </summary>
		public const string NamespacePubSubAccessAuthorize = "http://jabber.org/protocol/pubsub#access-authorize";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#access-open
		/// </summary>
		public const string NamespacePubSubAccessOpen = "http://jabber.org/protocol/pubsub#access-open";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#access-presence
		/// </summary>
		public const string NamespacePubSubAccessPresence = "http://jabber.org/protocol/pubsub#access-presence";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#access-roster
		/// </summary>
		public const string NamespacePubSubAccessRoster = "http://jabber.org/protocol/pubsub#access-roster";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#access-whitelist
		/// </summary>
		public const string NamespacePubSubAccessWhitelist = "http://jabber.org/protocol/pubsub#access-whitelist";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#collections
		/// </summary>
		public const string NamespacePubSubCollections = "http://jabber.org/protocol/pubsub#collections";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#config-node
		/// </summary>
		public const string NamespacePubSubNodeConfiguration = "http://jabber.org/protocol/pubsub#config-node";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#create-and-configure
		/// </summary>
		public const string NamespacePubSubCreateAndConfigure = "http://jabber.org/protocol/pubsub#create-and-configure";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#create-nodes
		/// </summary>
		public const string NamespacePubSubCreateNodes = "http://jabber.org/protocol/pubsub#create-nodes";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#delete-items
		/// </summary>
		public const string NamespacePubSubDeleteItems = "http://jabber.org/protocol/pubsub#delete-items";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#delete-nodes
		/// </summary>
		public const string NamespacePubSubDeleteNodes = "http://jabber.org/protocol/pubsub#delete-nodes";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#item-ids
		/// </summary>
		public const string NamespacePubSubItemIds = "http://jabber.org/protocol/pubsub#item-ids";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#last-published
		/// </summary>
		public const string NamespacePubSubLastPublished = "http://jabber.org/protocol/pubsub#last-published";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#leased-subscription
		/// </summary>
		public const string NamespacePubSubLeasedSubscription = "http://jabber.org/protocol/pubsub#leased-subscription";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#manage-subscriptions
		/// </summary>
		public const string NamespacePubSubManageSubscriptions = "http://jabber.org/protocol/pubsub#manage-subscriptions";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#member-affiliation
		/// </summary>
		public const string NamespacePubSubemberAffiliation = "http://jabber.org/protocol/pubsub#member-affiliation";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#meta-data
		/// </summary>
		public const string NamespacePubSubMetaData = "http://jabber.org/protocol/pubsub#meta-data";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#modify-affiliations
		/// </summary>
		public const string NamespacePubSubModifyAffiliations = "http://jabber.org/protocol/pubsub#modify-affiliations";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#multi-collection
		/// </summary>
		public const string NamespacePubSubMultiCollection = "http://jabber.org/protocol/pubsub#multi-collection";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#multi-items
		/// </summary>
		public const string NamespacePubSubMultiItems = "http://jabber.org/protocol/pubsub#multi-items";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#outcast-affiliation
		/// </summary>
		public const string NamespacePubSubOutcastAffiliation = "http://jabber.org/protocol/pubsub#outcast-affiliation";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#persistent-items
		/// </summary>
		public const string NamespacePubSubPersistentItems = "http://jabber.org/protocol/pubsub#persistent-items";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#presence-subscribe
		/// </summary>
		public const string NamespacePubSubPresenceSubscribe = "http://jabber.org/protocol/pubsub#presence-subscribe";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#publish
		/// </summary>
		public const string NamespacePubSubPublish = "http://jabber.org/protocol/pubsub#publish";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#publish-only-affiliation
		/// </summary>
		public const string NamespacePubSubPublishOnlyAffiliation = "http://jabber.org/protocol/pubsub#publish-only-affiliation";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#publisher-affiliation
		/// </summary>
		public const string NamespacePubSubPublisheAffiliation = "http://jabber.org/protocol/pubsub#publisher-affiliation";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#purge-nodes
		/// </summary>
		public const string NamespacePubSubPurgeNodes = "http://jabber.org/protocol/pubsub#purge-nodes";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retract-items
		/// </summary>
		public const string NamespacePubSubRetractItems = "http://jabber.org/protocol/pubsub#retract-items";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retrieve-affiliations
		/// </summary>
		public const string NamespacePubSubRetrieveAffiliations = "http://jabber.org/protocol/pubsub#retrieve-affiliations";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retrieve-default
		/// </summary>
		public const string NamespacePubSubRetrieveDefault = "http://jabber.org/protocol/pubsub#retrieve-default";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retrieve-default-sub
		/// </summary>
		public const string NamespacePubSubRetrieveDefaultSub = "http://jabber.org/protocol/pubsub#retrieve-default-sub";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retrieve-items
		/// </summary>
		public const string NamespacePubSubRetrieveItems = "http://jabber.org/protocol/pubsub#retrieve-items";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#retrieve-subscriptions
		/// </summary>
		public const string NamespacePubSubRetrieveSubscriptions = "http://jabber.org/protocol/pubsub#retrieve-subscriptions";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#subscribe
		/// </summary>
		public const string NamespacePubSubSubscribe = "http://jabber.org/protocol/pubsub#subscribe";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#subscription-options
		/// </summary>
		public const string NamespacePubSubSubscriptionOptions = "http://jabber.org/protocol/pubsub#subscription-options";

		/// <summary>
		/// http://jabber.org/protocol/pubsub#subscription-notifications
		/// </summary>
		public const string NamespacePubSubSubscriptionNotifications = "http://jabber.org/protocol/pubsub#subscription-notifications";

		private readonly string componentAddress;

		/// <summary>
		/// Client managing communication with a Publish/Subscribe component.
		/// https://xmpp.org/extensions/xep-0060.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		/// <param name="ComponentAddress">Address to the Publish/Subscribe component.</param>
		public PubSubClient(XmppClient Client, string ComponentAddress)
			: base(Client)
		{
			this.componentAddress = ComponentAddress;

			this.Client.RegisterMessageHandler("event", NamespacePubSubEvents, this.EventNotificationHandler, true);
			this.Client.RegisterMessageFormHandler(FormTypeSubscriptionAuthorization, this.SubscriptionAuthorizationHandler);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.Client.UnregisterMessageHandler("event", NamespacePubSubEvents, this.EventNotificationHandler, true);
			this.Client.UnregisterMessageFormHandler(FormTypeSubscriptionAuthorization, this.SubscriptionAuthorizationHandler);

			base.Dispose();
		}

		/// <summary>
		/// Publish/Subscribe component address.
		/// </summary>
		public string ComponentAddress => this.componentAddress;

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0060" };

		#region Create Node

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task CreateNode(string Name, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.CreateNode(this.componentAddress, Name, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task CreateNode(string Name, NodeConfiguration Configuration,
			EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.CreateNode(this.componentAddress, Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task CreateNode(string Name, DataForm Configuration,
			EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.CreateNode(this.componentAddress, Name, Configuration, Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task CreateNode(string ServiceAddress, string Name, NodeConfiguration Configuration,
			EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.CreateNode(ServiceAddress, Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Creates a node on the server.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="Configuration">Node configuration.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task CreateNode(string ServiceAddress, string Name, DataForm Configuration, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><create node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'/>");

			if (!(Configuration is null))
			{
				Xml.Append("<configure>");
				Configuration.SerializeSubmit(Xml);
				Xml.Append("</configure>");
			}

			Xml.Append("</pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new NodeEventArgs(Name, e)), State);
		}

		#endregion

		#region Configure Node

		/// <summary>
		/// Gets the configuration for a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetNodeConfiguration(string Name, EventHandlerAsync<ConfigurationEventArgs> Callback, object State)
		{
			return this.GetNodeConfiguration(this.componentAddress, Name, Callback, State);
		}

		/// <summary>
		/// Gets the configuration for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetNodeConfiguration(string ServiceAddress, string Name, EventHandlerAsync<ConfigurationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'/>");
			Xml.Append("</pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				NodeConfiguration Configuration = null;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "configure" &&
							XML.Attribute(E2, "node") == Name)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
								{
									Form = new DataForm(this.client, E3, this.ConfigureNode, this.CancelConfiguration, e.From, e.To)
									{
										State = Name
									};
								}
							}
						}
					}
				}

				if (Form is null)
					e.Ok = false;
				else
					Configuration = new NodeConfiguration(Form);

				return Callback.Raise(this, new ConfigurationEventArgs(Name, Configuration, new DataFormEventArgs(Form, e)));
			}, State);
		}

		private Task ConfigureNode(object Sender, DataForm Form)
		{
			string Name = (string)Form.State;
			return this.ConfigureNode(Name, Form, null, null);
		}

		private Task CancelConfiguration(object Sender, DataForm Form)
		{
			string Name = (string)Form.State;
			return this.CancelNodeConfiguration(Name, null, null);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task ConfigureNode(string Name, NodeConfiguration Configuration,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureNode(this.componentAddress, Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task ConfigureNode(string Name, DataForm Configuration, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureNode(this.componentAddress, Name, Configuration, Callback, State);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task ConfigureNode(string ServiceAddress, string Name, NodeConfiguration Configuration,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.ConfigureNode(ServiceAddress, Name, Configuration.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Configures a publish/subscribe node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Node name</param>
		/// <param name="Configuration">Configuration</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task ConfigureNode(string ServiceAddress, string Name, DataForm Configuration,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'>");

			Configuration?.SerializeSubmit(Xml);

			Xml.Append("</configure></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new NodeEventArgs(Name, e)), State);
		}

		/// <summary>
		/// Cancels a node configuration.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task CancelNodeConfiguration(string Name, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.CancelNodeConfiguration(this.componentAddress, Name, Callback, State);
		}

		/// <summary>
		/// Cancels a node configuration.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task CancelNodeConfiguration(string ServiceAddress, string Name, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><configure node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'><x xmlns='");
			Xml.Append(XmppClient.NamespaceData);
			Xml.Append("' type='cancel'/></configure></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new NodeEventArgs(Name, e)), State);
		}

		/// <summary>
		/// Gets the default configuration for a node.
		/// </summary>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetDefaultNodeConfiguration(EventHandlerAsync<ConfigurationEventArgs> Callback, object State)
		{
			return this.GetDefaultNodeConfiguration(this.componentAddress, Callback, State);
		}

		/// <summary>
		/// Gets the default configuration for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Callback">Method to call when request has been processed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetDefaultNodeConfiguration(string ServiceAddress, EventHandlerAsync<ConfigurationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><default/>");
			Xml.Append("</pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				NodeConfiguration Configuration = null;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "default")
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
									Form = new DataForm(this.client, E3, this.ConfigureNode, this.CancelConfiguration, e.From, e.To);
							}
						}
					}
				}

				if (Form is null)
					e.Ok = false;
				else
					Configuration = new NodeConfiguration(Form);

				return Callback.Raise(this, new ConfigurationEventArgs(string.Empty, Configuration, new DataFormEventArgs(Form, e)));
			}, State);
		}

		#endregion

		#region Delete Node

		/// <summary>
		/// Deletes a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task DeleteNode(string Name, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.DeleteNode(this.componentAddress, Name, null, Callback, State);
		}

		/// <summary>
		/// Deletes a node.
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="RedirectUrl">Redirection URL.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task DeleteNode(string Name, string RedirectUrl, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.DeleteNode(this.componentAddress, Name, RedirectUrl, Callback, State);
		}

		/// <summary>
		/// Deletes a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="RedirectUrl">Redirection URL.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task DeleteNode(string ServiceAddress, string Name, string RedirectUrl, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><delete node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'>");

			if (!string.IsNullOrEmpty(RedirectUrl))
			{
				Xml.Append("<redirect uri='");
				Xml.Append(XML.Encode(RedirectUrl));
				Xml.Append("'/>");
			}

			Xml.Append("</delete></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new NodeEventArgs(Name, e)), State);
		}

		#endregion

		#region Subscribe to node

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, null, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, string Jid, EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, Jid, (DataForm)null, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, SubscriptionOptions Options,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, DataForm Options,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, null, Options, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, string Jid, SubscriptionOptions Options,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, Jid, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string NodeName, string Jid, DataForm Options,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Subscribe(this.componentAddress, NodeName, Jid, Options, Callback, State);
		}

		/// <summary>
		/// Subscribes to a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Subscribe(string ServiceAddress, string NodeName, string Jid, DataForm Options,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><subscribe node='");
			Xml.Append(XML.Encode(NodeName));

			if (!string.IsNullOrEmpty(Jid))
			{
				Xml.Append("' jid='");
				Xml.Append(XML.Encode(Jid));
			}

			Xml.Append("'/>");

			if (!(Options is null))
			{
				Xml.Append("<options node='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append("' jid='");
				Xml.Append(XML.Encode(Jid));
				Xml.Append("'>");
				Options.SerializeSubmit(Xml);
				Xml.Append("</options>");
			}

			Xml.Append("</pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				SubscriptionOptions Options2 = null;
				OptionsAvailability Availability = OptionsAvailability.Unknown;
				NodeSubscriptionStatus Status = NodeSubscriptionStatus.none;
				DateTime Expires = DateTime.MaxValue;
				DataForm Form = null;
				XmlElement E;
				string SubscriptionId = string.Empty;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.NamespaceURI == NamespacePubSub)
						{
							switch (E2.LocalName)
							{
								case "subscription":
									if (XML.Attribute(E2, "node") != NodeName)
									{
										e.Ok = false;
										break;
									}

									if (XML.Attribute(E2, "jid") != Jid)
									{
										e.Ok = false;
										break;
									}

									Status = XML.Attribute(E2, "subscription", NodeSubscriptionStatus.none);
									Expires = XML.Attribute(E2, "expiry", DateTime.MaxValue);
									SubscriptionId = XML.Attribute(E2, "subid");

									foreach (XmlNode N2 in E2.ChildNodes)
									{
										if (N2 is XmlElement E3)
										{
											switch (E3.LocalName)
											{
												case "subscribe-options":
													Availability = OptionsAvailability.Supported;
													foreach (XmlNode N3 in E3.ChildNodes)
													{
														if (N3.LocalName == "required")
															Availability = OptionsAvailability.Required;
													}
													break;

												case "options":
													Availability = OptionsAvailability.Supported;
													foreach (XmlNode N3 in E3.ChildNodes)
													{
														if (N3 is XmlElement E4 && E4.LocalName == "x")
														{
															Form = new DataForm(this.client, E4, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
															{
																State = new object[] { NodeName, Jid, SubscriptionId }
															};
															Options2 = new SubscriptionOptions(Form);
														}
													}
													break;
											}
										}
									}
									break;

								case "options":
									Availability = OptionsAvailability.Supported;
									foreach (XmlNode N3 in E2.ChildNodes)
									{
										if (N3 is XmlElement E4 && E4.LocalName == "x")
										{
											Form = new DataForm(this.client, E4, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
											{
												State = new object[] { NodeName, Jid, SubscriptionId }
											};
											Options2 = new SubscriptionOptions(Form);
										}
									}
									break;
							}
						}
					}
				}
				else
					e.Ok = false;

				return Callback.Raise(this, new SubscriptionEventArgs(NodeName, Jid, SubscriptionId,
					Options2, Availability, Expires, Status, new DataFormEventArgs(Form, e)));

			}, State);
		}

		private Task SubmitSubscribeOptions(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			string NodeName = (string)P[0];
			string Jid = (string)P[1];
			string SubscrriptionId = (string)P[2];

			return this.SetSubscriptionOptions(NodeName, Jid, SubscrriptionId, Form, null, null);
		}

		private Task CancelSubscribeOptions(object Sender, DataForm Form)
		{
			return Task.CompletedTask;  // Do nothing.
		}

		#endregion

		#region Subscription options

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptionOptions(string NodeName,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.GetSubscriptionOptions(this.componentAddress, NodeName, null, null, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptionOptions(string NodeName, string Jid,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.GetSubscriptionOptions(this.componentAddress, NodeName, Jid, null, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.GetSubscriptionOptions(this.componentAddress, NodeName, Jid, SubscriptionId,
				Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptionOptions(string ServiceAddress, string NodeName, string Jid, string SubscriptionId,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><options node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'/></pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				SubscriptionOptions Options = null;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "options" &&
							XML.Attribute(E2, "node") == NodeName &&
							XML.Attribute(E2, "jid") == Jid)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
								{
									Form = new DataForm(this.client, E3, this.SubmitSubscribeOptions, this.CancelSubscribeOptions, e.From, e.To)
									{
										State = new object[] { NodeName, Jid }
									};
								}
							}
						}
					}
				}

				if (Form is null)
					e.Ok = false;
				else
					Options = new SubscriptionOptions(Form);

				return Callback.Raise(this, new SubscriptionOptionsEventArgs(NodeName, Jid, Options,
					new DataFormEventArgs(Form, e)));

			}, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, SubscriptionOptions Options,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, null, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, string Jid, SubscriptionOptions Options,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, Jid, null, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, DataForm Options,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, null, null, Options, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, string Jid,
			DataForm Options, EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, Jid, null, Options, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId, SubscriptionOptions Options,
			EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, Jid, SubscriptionId, Options.ToForm(this), Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string NodeName, string Jid, string SubscriptionId,
			DataForm Options, EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.SetSubscriptionOptions(this.componentAddress, NodeName, Jid, SubscriptionId, Options, Callback, State);
		}

		/// <summary>
		/// Gets available subscription options.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options to set.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SetSubscriptionOptions(string ServiceAddress, string NodeName, string Jid, string SubscriptionId,
			DataForm Options, EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><options node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'>");
			Options.SerializeSubmit(Xml);
			Xml.Append("</options></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new SubscriptionOptionsEventArgs(
				NodeName, Jid, new SubscriptionOptions(Options), new DataFormEventArgs(Options, e))), State);
		}

		/// <summary>
		/// Gets default subscription options.
		/// </summary>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetDefaultSubscriptionOptions(EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.GetDefaultSubscriptionOptions(null, Callback, State);
		}

		/// <summary>
		/// Gets default subscription options.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetDefaultSubscriptionOptions(string NodeName, EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			return this.GetDefaultSubscriptionOptions(this.componentAddress, NodeName, Callback, State);
		}

		/// <summary>
		/// Gets default subscription options.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetDefaultSubscriptionOptions(string ServiceAddress, string NodeName, EventHandlerAsync<SubscriptionOptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><default");

			if (!string.IsNullOrEmpty(NodeName))
			{
				Xml.Append(" node ='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append('\'');
			}

			Xml.Append("/></pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				XmlElement E;
				DataForm Form = null;
				SubscriptionOptions Options = null;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "default" &&
							XML.Attribute(E2, "node", NodeName) == NodeName)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "x")
									Form = new DataForm(this.client, E3, null, null, e.From, e.To);
							}
						}
					}
				}

				if (Form is null)
					e.Ok = false;
				else
					Options = new SubscriptionOptions(Form);

				return Callback.Raise(this, new SubscriptionOptionsEventArgs(NodeName, null, Options,
					new DataFormEventArgs(Form, e)));

			}, State);
		}

		#endregion

		#region Unsubscribe from node

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Unsubscribe(string NodeName, EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Unsubscribe(this.componentAddress, NodeName, null, null, Callback, State);
		}

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Unsubscribe(string NodeName, string Jid, EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Unsubscribe(this.componentAddress, NodeName, Jid, null, Callback, State);
		}

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Unsubscribe(string NodeName, string Jid, string SubscriptionId,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			return this.Unsubscribe(this.componentAddress, NodeName, Jid, SubscriptionId, Callback, State);
		}

		/// <summary>
		/// Unsubscribes from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID, if different from the bare JID of the client.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Unsubscribe(string ServiceAddress, string NodeName, string Jid, string SubscriptionId,
			EventHandlerAsync<SubscriptionEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			if (string.IsNullOrEmpty(Jid))
				Jid = this.client.BareJID;

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><unsubscribe node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(Jid));

			if (!string.IsNullOrEmpty(SubscriptionId))
			{
				Xml.Append("' subid='");
				Xml.Append(XML.Encode(SubscriptionId));
			}

			Xml.Append("'/></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				NodeSubscriptionStatus Status = NodeSubscriptionStatus.none;
				DateTime Expires = DateTime.MaxValue;
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.NamespaceURI == NamespacePubSub)
						{
							switch (E2.LocalName)
							{
								case "subscription":
									if (XML.Attribute(E2, "node") != NodeName)
									{
										e.Ok = false;
										break;
									}

									if (XML.Attribute(E2, "jid") != Jid)
									{
										e.Ok = false;
										break;
									}

									if (!string.IsNullOrEmpty(SubscriptionId) &&
										XML.Attribute(E2, "subid", SubscriptionId) != SubscriptionId)
									{
										e.Ok = false;
										break;
									}

									Status = XML.Attribute(E2, "subscription", NodeSubscriptionStatus.none);
									Expires = XML.Attribute(E2, "expiry", DateTime.MaxValue);
									SubscriptionId = XML.Attribute(E2, "subid");
									break;
							}
						}
					}
				}
				else
					e.Ok = false;

				return Callback.Raise(this, new SubscriptionEventArgs(NodeName, Jid, SubscriptionId,
					null, OptionsAvailability.Unknown, Expires, Status, new DataFormEventArgs(Form, e)));

			}, State);
		}

		#endregion

		#region Publish Item

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Publish(string Node, EventHandlerAsync<ItemResultEventArgs> Callback, object State)
		{
			return this.Publish(this.componentAddress, Node, string.Empty, string.Empty, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Publish(string Node, string PayloadXml, EventHandlerAsync<ItemResultEventArgs> Callback, object State)
		{
			return this.Publish(this.componentAddress, Node, string.Empty, PayloadXml, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity, if available. If used, and an existing item
		/// is available with that identity, it will be updated with the new content.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Publish(string Node, string ItemId, string PayloadXml, EventHandlerAsync<ItemResultEventArgs> Callback, object State)
		{
			return this.Publish(this.componentAddress, Node, ItemId, PayloadXml, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity, if available. If used, and an existing item
		/// is available with that identity, it will be updated with the new content.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Publish(string ServiceAddress, string Node, string ItemId, string PayloadXml, EventHandlerAsync<ItemResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><publish node='");
			Xml.Append(XML.Encode(Node));

			if (string.IsNullOrEmpty(ItemId) && string.IsNullOrEmpty(PayloadXml))
				Xml.Append("'/>");
			else
			{
				Xml.Append("'><item");

				if (!string.IsNullOrEmpty(ItemId))
				{
					Xml.Append(" id='");
					Xml.Append(XML.Encode(ItemId));
					Xml.Append('\'');
				}

				if (string.IsNullOrEmpty(PayloadXml))
					Xml.Append("/>");
				else
				{
					Xml.Append('>');
					Xml.Append(PayloadXml);
					Xml.Append("</item>");
				}

				Xml.Append("</publish>");
			}

			Xml.Append("</pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				string NodeName = string.Empty;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" &&
					E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N2 in E.ChildNodes)
					{
						if (N2 is XmlElement E2 && E2.LocalName == "publish")
						{
							NodeName = XML.Attribute(E2, "node");

							foreach (XmlNode N3 in E2.ChildNodes)
							{
								if (N3 is XmlElement E3 && E3.LocalName == "item")
								{
									ItemId = XML.Attribute(E3, "id");
									break;
								}
							}
						}
					}
				}

				return Callback.Raise(this, new ItemResultEventArgs(NodeName, ItemId, e));
			}, State);
		}

		private async Task EventNotificationHandler(object Sender, MessageEventArgs e)
		{
			string SubscriptionId = string.Empty;
			string ReplyTo = e.From;
			DateTime? Delay = null;

			foreach (XmlNode N in e.Message.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "headers":
							if (E.NamespaceURI == NamespaceStanzaHeaders)
							{
								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "header" && E2.NamespaceURI == NamespaceStanzaHeaders
										&& XML.Attribute(E2, "name") == "SubID")
									{
										SubscriptionId = E2.InnerText;
										break;
									}
								}

								if (!string.IsNullOrEmpty(SubscriptionId))
									break;
							}
							break;

						case "delay":
							if (E.NamespaceURI == NamespaceDelayedDelivery &&
								E.HasAttribute("stamp") &&
								XML.TryParse(E.GetAttribute("stamp"), out DateTime Timestamp))
							{
								Delay = Timestamp;
							}
							break;

						case "addresses":
							if (E.NamespaceURI == "http://jabber.org/protocol/address")
							{
								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "address" && XML.Attribute(E2, "type") == "replyto")
									{
										ReplyTo = XML.Attribute(E2, "jid");
										break;
									}
								}

							}
							break;
					}
				}
			}

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "items":
							string NodeName = XML.Attribute(E, "node");

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									switch (E2.LocalName)
									{
										case "item":
											string ItemId = XML.Attribute(E2, "id");
											string Publisher = XML.Attribute(E2, "publisher");
											ItemNotificationEventArgs e2 = new ItemNotificationEventArgs(NodeName, ItemId, SubscriptionId, Publisher, ReplyTo, E2, Delay, e);

											await this.ItemNotification.Raise(this, e2);
											break;

										case "retract":
											ItemId = XML.Attribute(E2, "id");
											e2 = new ItemNotificationEventArgs(NodeName, ItemId, SubscriptionId, string.Empty, ReplyTo, E2, Delay, e);

											await this.ItemRetracted.Raise(this, e2);
											break;
									}
								}
							}
							break;

						case "purge":
							NodeName = XML.Attribute(E, "node");
							NodeNotificationEventArgs e3 = new NodeNotificationEventArgs(NodeName, SubscriptionId, e);

							await this.NodePurged.Raise(this, e3);
							break;

						case "subscription":
							NodeName = XML.Attribute(E, "node");
							string Jid = XML.Attribute(E, "jid");
							NodeSubscriptionStatus Status = XML.Attribute(E, "subscription", NodeSubscriptionStatus.none);

							await this.SubscriptionStatusChanged.Raise(this, new SubscriptionNotificationEventArgs(NodeName, Jid, Status, e));
							break;

						case "affiliations":
							NodeName = XML.Attribute(E, "node");

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									switch (E2.LocalName)
									{
										case "affiliation":
											Jid = XML.Attribute(E2, "jid");
											string s = XML.Attribute(E2, "affiliation");
											if (!TryParse(s, out AffiliationStatus Affiliation))
												break;

											await this.AffiliationNotification.Raise(this, new AffiliationNotificationEventArgs(NodeName, Jid, Affiliation, e));
											break;
									}
								}
							}
							break;

					}
				}
			}
		}

		/// <summary>
		/// Event raised whenever an item notification has been received.
		/// </summary>
		public event EventHandlerAsync<ItemNotificationEventArgs> ItemNotification = null;

		/// <summary>
		/// Tries to parse an affiliation
		/// </summary>
		/// <param name="s">String representation</param>
		/// <param name="Affiliation">Affiliation</param>
		/// <returns>If parsing was successful.</returns>
		private static bool TryParse(string s, out AffiliationStatus Affiliation)
		{
			if (s == "publish-only")
			{
				Affiliation = AffiliationStatus.publishOnly;
				return true;
			}
			else
				return Enum.TryParse(s, out Affiliation);
		}

		#endregion

		#region Retract Item

		/// <summary>
		/// Retracts an item from a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Retract(string Node, string ItemId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Retract(this.componentAddress, Node, ItemId, Callback, State);
		}

		/// <summary>
		/// Retracts an item from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Retract(string ServiceAddress, string Node, string ItemId, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><retract node='");
			Xml.Append(XML.Encode(Node));
			Xml.Append("'><item");
			Xml.Append(" id='");
			Xml.Append(XML.Encode(ItemId));
			Xml.Append("'/></retract></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Event raised whenever an item retraction notification has been received.
		/// </summary>
		public event EventHandlerAsync<ItemNotificationEventArgs> ItemRetracted = null;

		#endregion

		#region Get Items

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string NodeName, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(this.componentAddress, NodeName, null, null, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Page">Query restriction, for pagination.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string NodeName, RestrictedQuery Page, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(this.componentAddress, NodeName, null, Page, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="ItemIds">Item identities.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string NodeName, string[] ItemIds, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(this.componentAddress, NodeName, ItemIds, null, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string ServiceAddress, string NodeName, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(ServiceAddress, NodeName, null, null, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Page">Query restriction, for pagination.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string ServiceAddress, string NodeName, RestrictedQuery Page, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(ServiceAddress, NodeName, null, Page, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="ItemIds">Item identities.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetItems(string ServiceAddress, string NodeName, string[] ItemIds, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(ServiceAddress, NodeName, ItemIds, null, null, Callback, State);
		}

		/// <summary>
		/// Gets items from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="ItemIds">Item identities.</param>
		/// <param name="Page">Query restriction, for pagination.</param>
		/// <param name="Count">Get this number of latest items.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		private Task GetItems(string ServiceAddress, string NodeName, string[] ItemIds, RestrictedQuery Page, int? Count, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);
			Xml.Append("'><items node='");
			Xml.Append(XML.Encode(NodeName));

			if (Count.HasValue)
			{
				Xml.Append("' max_items='");
				Xml.Append(Count.Value.ToString());
			}

			if (ItemIds is null)
				Xml.Append("'/>");
			else
			{
				Xml.Append("'>");

				foreach (string ItemId in ItemIds)
				{
					Xml.Append("<item id='");
					Xml.Append(XML.Encode(ItemId));
					Xml.Append("'/>");
				}

				Xml.Append("</items>");
			}

			Page?.Append(Xml);

			Xml.Append("</pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				List<PubSubItem> Items = new List<PubSubItem>();
				ResultPage ResultPage = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
						{
							switch (E2.LocalName)
							{
								case "items":
									if (E2.NamespaceURI == NamespacePubSub &&
										XML.Attribute(E2, "node") == NodeName)
									{
										foreach (XmlNode N2 in E2.ChildNodes)
										{
											if (N2 is XmlElement E3 && E3.LocalName == "item")
												Items.Add(new PubSubItem(ServiceAddress, NodeName, E3));
										}
									}
									break;

								case "set":
									ResultPage = new ResultPage(E2);
									break;
							}
						}
					}
				}

				return Callback.Raise(this, new ItemsEventArgs(NodeName, Items.ToArray(), ResultPage, e));

			}, State);
		}

		/// <summary>
		/// Gets the latest items from a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Count">Number of items to get.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetLatestItems(string NodeName, int Count, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(this.componentAddress, NodeName, null, null, Count, Callback, State);
		}

		/// <summary>
		/// Gets the latest items from a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Count">Number of items to get.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetLatestItems(string ServiceAddress, string NodeName, int Count, EventHandlerAsync<ItemsEventArgs> Callback, object State)
		{
			return this.GetItems(ServiceAddress, NodeName, null, null, Count, Callback, State);
		}

		#endregion

		#region Purge Node

		/// <summary>
		/// Purges a node (deletes all items persisted on the node).
		/// </summary>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task PurgeNode(string Name, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			return this.PurgeNode(this.componentAddress, Name, Callback, State);
		}

		/// <summary>
		/// Purges a node (deletes all items persisted on the node).
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="Name">Name of node.</param>
		/// <param name="Callback">Method to call when operation has completed.</param>
		/// <param name="State">State parameter to send to the callback method.</param>
		public Task PurgeNode(string ServiceAddress, string Name, EventHandlerAsync<NodeEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><purge node='");
			Xml.Append(XML.Encode(Name));
			Xml.Append("'/></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), (Sender, e) => Callback.Raise(this, new NodeEventArgs(Name, e)), State);
		}

		/// <summary>
		/// Event raised whenever a node has been purged and all its items have been deleted.
		/// </summary>
		public event EventHandlerAsync<NodeNotificationEventArgs> NodePurged = null;

		#endregion

		#region Subscription requests

		private Task SubscriptionAuthorizationHandler(object Sender, MessageFormEventArgs e)
		{
			if (e.From != this.componentAddress)
				return Task.CompletedTask;

			DataForm Form = e.Form;
			string SubscriptionId = Form["pubsub#subid"]?.ValueString ?? string.Empty;
			string NodeName = Form["pubsub#node"]?.ValueString ?? string.Empty;
			string Jid = Form["pubsub#subscriber_jid"]?.ValueString ?? string.Empty;

			return this.SubscriptionRequest.Raise(this, new SubscriptionRequestEventArgs(NodeName, Jid, SubscriptionId, e));
		}

		/// <summary>
		/// Event raised when a subscription request has been received on a node to 
		/// which the client is an owner.
		/// </summary>
		public event EventHandlerAsync<SubscriptionRequestEventArgs> SubscriptionRequest = null;

		/// <summary>
		/// Event raised when the status changes for a subscription.
		/// </summary>
		public event EventHandlerAsync<SubscriptionNotificationEventArgs> SubscriptionStatusChanged = null;

		#endregion

		#region Subscription management

		/// <summary>
		/// Gets the list of subscriptions for a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptions(string NodeName, EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			return this.GetSubscriptions(this.componentAddress, NodeName, Callback, State);
		}

		/// <summary>
		/// Gets the list of subscriptions for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetSubscriptions(string ServiceAddress, string NodeName, EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><subscriptions node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("'/></pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				List<Subscription> Subscriptions = new List<Subscription>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "subscriptions" && XML.Attribute(E2, "node") == NodeName)
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "subscription")
								{
									Subscriptions.Add(new Subscription(NodeName,
										XML.Attribute(E3, "jid"),
										XML.Attribute(E3, "subscription", NodeSubscriptionStatus.none),
										XML.Attribute(E3, "subid")));
								}
							}
						}
					}
				}

				return Callback.Raise(this, new SubscriptionsEventArgs(NodeName, Subscriptions.ToArray(), e));
			}, State);
		}

		/// <summary>
		/// Updates subcriptions on a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Subscriptions">Subscriptions to be updated</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task UpdateSubscriptions(string NodeName, IEnumerable<KeyValuePair<string, NodeSubscriptionStatus>> Subscriptions,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.UpdateSubscriptions(this.componentAddress, NodeName, Subscriptions, Callback, State);
		}

		/// <summary>
		/// Updates subcriptions on a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Subscriptions">Subscriptions to be updated</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task UpdateSubscriptions(string ServiceAddress, string NodeName, IEnumerable<KeyValuePair<string, NodeSubscriptionStatus>> Subscriptions,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><subscriptions node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("'>");

			foreach (KeyValuePair<string, NodeSubscriptionStatus> Subscription in Subscriptions)
			{
				Xml.Append("<subscription jid='");
				Xml.Append(XML.Encode(Subscription.Key));
				Xml.Append("' subscription='");
				Xml.Append(Subscription.Value.ToString());
				Xml.Append("'/>");
			}

			Xml.Append("</subscriptions></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), Callback, State);
		}

		#endregion

		#region Affiliation management

		/// <summary>
		/// Gets the list of affiliations for a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetAffiliations(string NodeName, EventHandlerAsync<AffiliationsEventArgs> Callback, object State)
		{
			return this.GetAffiliations(this.componentAddress, NodeName, Callback, State);
		}

		/// <summary>
		/// Gets the list of affiliations for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetAffiliations(string ServiceAddress, string NodeName, EventHandlerAsync<AffiliationsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><affiliations node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("'/></pubsub>");

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				List<Affiliation> Affiliations = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSubOwner)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "affiliations" && XML.Attribute(E2, "node") == NodeName)
						{
							Affiliations = new List<Affiliation>();

							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "affiliation")
								{
									string s = XML.Attribute(E3, "affiliation");
									if (!TryParse(s, out AffiliationStatus Affiliation))
										continue;

									Affiliations.Add(new Affiliation(NodeName, XML.Attribute(E3, "jid"), Affiliation));
								}
							}
						}
					}
				}

				return Callback.Raise(this, new AffiliationsEventArgs(NodeName, Affiliations?.ToArray(), e));
			}, State);
		}

		/// <summary>
		/// Updates affiliations on a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Affiliations">Affiliations to be updated</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task UpdateAffiliations(string NodeName, IEnumerable<KeyValuePair<string, AffiliationStatus>> Affiliations,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.UpdateAffiliations(this.componentAddress, NodeName, Affiliations, Callback, State);
		}

		/// <summary>
		/// Updates affiliations on a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Affiliations">Affiliations to be updated</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task UpdateAffiliations(string ServiceAddress, string NodeName, IEnumerable<KeyValuePair<string, AffiliationStatus>> Affiliations,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSubOwner);
			Xml.Append("'><affiliations node='");
			Xml.Append(XML.Encode(NodeName));
			Xml.Append("'>");

			foreach (KeyValuePair<string, AffiliationStatus> Affiliation in Affiliations)
			{
				Xml.Append("<affiliation jid='");
				Xml.Append(XML.Encode(Affiliation.Key));
				Xml.Append("' affiliation='");

				if (Affiliation.Value == AffiliationStatus.publishOnly)
					Xml.Append("publish-only");
				else
					Xml.Append(Affiliation.Value.ToString());

				Xml.Append("'/>");
			}

			Xml.Append("</affiliations></pubsub>");

			return this.client.SendIqSet(ServiceAddress, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Event raised whenever the affiliation status of the client has changed on a node.
		/// </summary>
		public event EventHandlerAsync<AffiliationNotificationEventArgs> AffiliationNotification = null;

		#endregion

		#region My subscriptions

		/// <summary>
		/// Gets the list of your subscriptions.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMySubscriptions(EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			return this.GetMySubscriptions(this.componentAddress, null, Callback, State);
		}

		/// <summary>
		/// Gets the list of your subscriptions for a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMySubscriptions(string NodeName, EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			return this.GetMySubscriptions(this.componentAddress, NodeName, Callback, State);
		}

		/// <summary>
		/// Gets the list of your subscriptions for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMySubscriptions(string ServiceAddress, string NodeName, EventHandlerAsync<SubscriptionsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);

			if (string.IsNullOrEmpty(NodeName))
				Xml.Append("'><subscriptions/></pubsub>");
			else
			{
				Xml.Append("'><subscriptions node='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append("'/></pubsub>");
			}

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				List<Subscription> Subscriptions = new List<Subscription>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "subscriptions")
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "subscription")
								{
									Subscriptions.Add(new Subscription(
										E3.HasAttribute("node") ? E3.GetAttribute("node") : NodeName,
										XML.Attribute(E3, "jid"),
										XML.Attribute(E3, "subscription", NodeSubscriptionStatus.none),
										XML.Attribute(E3, "subid")));
								}
							}
						}
					}
				}

				return Callback.Raise(this, new SubscriptionsEventArgs(NodeName, Subscriptions.ToArray(), e));

			}, State);
		}

		#endregion

		#region My affiliations

		/// <summary>
		/// Gets the list of your affiliations.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMyAffiliations(EventHandlerAsync<AffiliationsEventArgs> Callback, object State)
		{
			return this.GetMyAffiliations(this.componentAddress, null, Callback, State);
		}

		/// <summary>
		/// Gets the list of your affiliations for a node.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMyAffiliations(string NodeName, EventHandlerAsync<AffiliationsEventArgs> Callback, object State)
		{
			return this.GetMyAffiliations(this.componentAddress, NodeName, Callback, State);
		}

		/// <summary>
		/// Gets the list of your affiliations for a node.
		/// </summary>
		/// <param name="ServiceAddress">Publish/Subscribe service address.</param>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetMyAffiliations(string ServiceAddress, string NodeName, EventHandlerAsync<AffiliationsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<pubsub xmlns='");
			Xml.Append(NamespacePubSub);

			if (string.IsNullOrEmpty(NodeName))
				Xml.Append("'><affiliations/></pubsub>");
			else
			{
				Xml.Append("'><affiliations node='");
				Xml.Append(XML.Encode(NodeName));
				Xml.Append("'/></pubsub>");
			}

			return this.client.SendIqGet(ServiceAddress, Xml.ToString(), (Sender, e) =>
			{
				List<Affiliation> Affiliations = new List<Affiliation>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "pubsub" && E.NamespaceURI == NamespacePubSub)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "affiliations")
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "affiliation")
								{
									if (!TryParse(XML.Attribute(E3, "affiliation"), out AffiliationStatus Affiliation))
										continue;

									Affiliations.Add(new Affiliation(
										E3.HasAttribute("node") ? E3.GetAttribute("node") : NodeName,
										E3.HasAttribute("jid") ? E3.GetAttribute("jid") : this.client.BareJID,
										Affiliation));
								}
							}
						}
					}
				}

				return Callback.Raise(this, new AffiliationsEventArgs(NodeName, Affiliations.ToArray(), e));

			}, State);
		}

		#endregion

	}
}
