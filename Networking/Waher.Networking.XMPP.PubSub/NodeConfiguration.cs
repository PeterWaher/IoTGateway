using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Contains information about the configuration of a node.
	/// </summary>
	public class NodeConfiguration
	{
		private string bodyXsltUrl = null;
		private string dataFormXsltUrl = null;
		private string title = null;
		private string payloadType = null;
		private string[] description = null;
		private string[] collections = null;
		private string[] contact = null;
		private string[] childAssociationWhitelist = null;
		private string[] children = null;
		private string[] replyRooms = null;
		private string[] replyTo = null;
		private string[] rosterGroupsAllowed = null;
		private NodeAccessModel? accessModel = null;
		private NodeItemReply? itemReply = null;
		private NodeChildAssociationPolicy? childAssociationPolicy = null;
		private NodeType? nodeType = null;
		private PublisherModel? publisherModel = null;
		private SendLastPublishedItem? sendLastPublishedItem = null;
		private NotificationType? notificationType = null;
		private int? maxChildren = null;
		private int? maxItems = null;
		private int? maxPayloadSize = null;
		private int? itemExpireSeconds = null;
		private bool? deliverNotifications = null;
		private bool? deliverPayloads = null;
		private bool? notifyConfig = null;
		private bool? notifyDelete = null;
		private bool? notifyRetract = null;
		private bool? notifySubscriptions = null;
		private bool? persistItems = null;
		private bool? presenceBasedDelivery = null;
		private bool? allowSubscriptions = null;
		private bool? purgeOffline = null;

		/// <summary>
		/// Contains information about the configuration of a node.
		/// </summary>
		public NodeConfiguration()
		{
		}

		/// <summary>
		/// Contains information about the configuration of a node.
		/// </summary>
		public NodeConfiguration(DataForm Form)
		{
			foreach (Field F in Form.Fields)
			{
				switch (F.Var)
				{
					case "pubsub#access_model":
						if (Enum.TryParse<NodeAccessModel>(F.ValueString, out NodeAccessModel AccessModel))
							this.accessModel = AccessModel;
						break;

					case "pubsub#body_xslt":
						this.bodyXsltUrl = F.ValueString;
						break;

					case "pubsub#collection":
						this.collections = F.ValueStrings;
						break;

					case "pubsub#contact":
						this.contact = F.ValueStrings;
						break;

					case "pubsub#dataform_xslt":
						this.dataFormXsltUrl = F.ValueString;
						break;

					case "pubsub#deliver_notifications":
						if (CommonTypes.TryParse(F.ValueString, out bool b))
							this.deliverNotifications = b;
						break;

					case "pubsub#deliver_payloads":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.deliverPayloads = b;
						break;

					case "pubsub#itemreply":
						if (Enum.TryParse(F.ValueString, out NodeItemReply ItemReply))
							this.itemReply = ItemReply;
						break;

					case "pubsub#children_association_policy":
						if (Enum.TryParse(F.ValueString, out NodeChildAssociationPolicy ChildAssociationPolicy))
							this.childAssociationPolicy = ChildAssociationPolicy;
						break;

					case "pubsub#children_association_whitelist":
						this.childAssociationWhitelist = F.ValueStrings;
						break;

					case "pubsub#children":
						this.children = F.ValueStrings;
						break;

					case "pubsub#children_max":
						if (int.TryParse(F.ValueString, out int i) && i > 0)
							this.maxChildren = i;
						break;

					case "pubsub#max_items":
						if (int.TryParse(F.ValueString, out i) && i > 0)
							this.maxItems = i;
						break;

					case "pubsub#item_expire":
						if (int.TryParse(F.ValueString, out i) && i > 0)
							this.itemExpireSeconds = i;
						break;

					case "pubsub#max_payload_size":
						if (int.TryParse(F.ValueString, out i) && i > 0)
							this.maxPayloadSize = i;
						break;

					case "pubsub#node_type":
						if (Enum.TryParse(F.ValueString, out NodeType NodeType))
							this.nodeType = NodeType;
						break;

					case "pubsub#notify_config":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.notifyConfig = b;
						break;

					case "pubsub#notify_delete":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.notifyDelete = b;
						break;

					case "pubsub#notify_retract":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.notifyRetract = b;
						break;

					case "pubsub#notify_sub":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.notifySubscriptions = b;
						break;

					case "pubsub#persist_items":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.persistItems = b;
						break;

					case "pubsub#presence_based_delivery":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.presenceBasedDelivery = b;
						break;

					case "pubsub#publish_model":
						if (Enum.TryParse<PublisherModel>(F.ValueString, out PublisherModel PublisherModel))
							this.publisherModel = PublisherModel;
						break;

					case "pubsub#replyroom":
						this.replyRooms = F.ValueStrings;
						break;

					case "pubsub#replyto":
						this.replyTo = F.ValueStrings;
						break;

					case "pubsub#roster_groups_allowed":
						this.rosterGroupsAllowed = F.ValueStrings;
						break;

					case "pubsub#send_last_published_item":
						if (Enum.TryParse<SendLastPublishedItem>(F.ValueString, out SendLastPublishedItem SendLastPublishedItem))
							this.sendLastPublishedItem = SendLastPublishedItem;
						break;

					case "pubsub#subscribe":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.allowSubscriptions = b;
						break;

					case "pubsub#title":
						this.title = F.ValueString;
						break;

					case "pubsub#description":
						this.description = F.ValueStrings;
						break;

					case "pubsub#type":
						this.payloadType = F.ValueString;
						break;

					case "pubsub#notification_type":
						if (Enum.TryParse<NotificationType>(F.ValueString, out NotificationType NotificationType))
							this.notificationType = NotificationType;
						break;

					case "pubsub#purge_offline":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.purgeOffline = b;
						break;
				}
			}
		}

		/// <summary>
		/// Who may subscribe and retrieve items
		/// </summary>
		public NodeAccessModel? AccessModel
		{
			get { return this.accessModel; }
			set { this.accessModel = value; }
		}

		/// <summary>
		/// The URL of an XSL transformation which can be applied to payloads in order to generate an appropriate message body element.
		/// </summary>
		public string BodyXsltUrl
		{
			get { return this.bodyXsltUrl; }
			set { this.bodyXsltUrl = value; }
		}

		/// <summary>
		/// The URL of an XSL transformation which can be applied to the payload format in order to generate a valid Data Forms result that 
		/// the client could display using a generic Data Forms rendering engine.
		/// </summary>
		public string DataFormXsltUrl
		{
			get { return this.dataFormXsltUrl; }
			set { this.dataFormXsltUrl = value; }
		}

		/// <summary>
		/// The collection(s) with which a node is affiliated
		/// </summary>
		public string[] Collections
		{
			get { return this.collections; }
			set { this.collections = value; }
		}

		/// <summary>
		/// The JIDs of those to contact with questions
		/// </summary>
		public string[] Contact
		{
			get { return this.contact; }
			set { this.contact = value; }
		}

		/// <summary>
		/// Whether to deliver event notifications
		/// </summary>
		public bool? DeliverNotifications
		{
			get { return this.deliverNotifications; }
			set { this.deliverNotifications = value; }
		}

		/// <summary>
		/// Whether to deliver payloads with event notifications; applies only to leaf nodes
		/// </summary>
		public bool? DeliverPayloads
		{
			get { return this.deliverPayloads; }
			set { this.deliverPayloads = value; }
		}

		/// <summary>
		/// Whether owners or publisher should receive replies to items
		/// </summary>
		public NodeItemReply? ItemReply
		{
			get { return this.itemReply; }
			set { this.itemReply = value; }
		}

		/// <summary>
		/// Who may associate leaf nodes with a collection
		/// </summary>
		public NodeChildAssociationPolicy? ChildAssociationPolicy
		{
			get { return this.childAssociationPolicy; }
			set { this.childAssociationPolicy = value; }
		}

		/// <summary>
		/// The list of JIDs that may associate leaf nodes with a collection
		/// </summary>
		public string[] ChildAssociationWhitelist
		{
			get { return this.childAssociationWhitelist; }
			set { this.childAssociationWhitelist = value; }
		}

		/// <summary>
		/// The child nodes (leaf or collection) associated with a collection
		/// </summary>
		public string[] Children
		{
			get { return this.children; }
			set { this.children = value; }
		}

		/// <summary>
		/// The maximum number of child nodes that can be associated with a collection
		/// </summary>
		public int? MaxChildren
		{
			get { return this.maxChildren; }
			set { this.maxChildren = value; }
		}

		/// <summary>
		/// The maximum number of items to persist
		/// </summary>
		public int? MaxItems
		{
			get { return this.maxItems; }
			set { this.maxItems = value; }
		}

		/// <summary>
		/// The maximum number of items to persist
		/// </summary>
		public int? MaxPayloadSize
		{
			get { return this.maxPayloadSize; }
			set { this.maxPayloadSize = value; }
		}

		/// <summary>
		/// Whether the node is a leaf (default) or a collection
		/// </summary>
		public NodeType? NodeType
		{
			get { return this.nodeType; }
			set { this.nodeType = value; }
		}

		/// <summary>
		/// Whether to notify subscribers when the node configuration changes
		/// </summary>
		public bool? NotifyConfig
		{
			get { return this.notifyConfig; }
			set { this.notifyConfig = value; }
		}

		/// <summary>
		/// Whether to notify subscribers when the node is deleted
		/// </summary>
		public bool? NotifyDelete
		{
			get { return this.notifyDelete; }
			set { this.notifyDelete = value; }
		}

		/// <summary>
		/// Whether to notify subscribers when items are removed from the node
		/// </summary>
		public bool? NotifyRetract
		{
			get { return this.notifyRetract; }
			set { this.notifyRetract = value; }
		}

		/// <summary>
		/// Whether to notify owners about new subscribers and unsubscribes
		/// </summary>
		public bool? NotifySubscriptions
		{
			get { return this.notifySubscriptions; }
			set { this.notifySubscriptions = value; }
		}

		/// <summary>
		/// Whether to persist items to storage
		/// </summary>
		public bool? PersistItems
		{
			get { return this.persistItems; }
			set { this.persistItems = value; }
		}

		/// <summary>
		/// Whether to deliver notifications to available users only
		/// </summary>
		public bool? PresenceBasedDelivery
		{
			get { return this.presenceBasedDelivery; }
			set { this.presenceBasedDelivery = value; }
		}

		/// <summary>
		/// The publisher model
		/// </summary>
		public PublisherModel? PublisherModel
		{
			get { return this.publisherModel; }
			set { this.publisherModel = value; }
		}

		/// <summary>
		/// The specific multi-user chat rooms to specify for replyroom
		/// </summary>
		public string[] ReplyRooms
		{
			get { return this.replyRooms; }
			set { this.replyRooms = value; }
		}

		/// <summary>
		/// The specific JID(s) to specify for replyto
		/// </summary>
		public string[] ReplyTo
		{
			get { return this.replyTo; }
			set { this.replyTo = value; }
		}

		/// <summary>
		/// The roster group(s) allowed to subscribe and retrieve items
		/// </summary>
		public string[] RosterGroupsAllowed
		{
			get { return this.rosterGroupsAllowed; }
			set { this.rosterGroupsAllowed = value; }
		}

		/// <summary>
		/// When to send the last published item
		/// </summary>
		public SendLastPublishedItem? SendLastPublishedItem
		{
			get { return this.sendLastPublishedItem; }
			set { this.sendLastPublishedItem = value; }
		}

		/// <summary>
		/// Whether to allow subscriptions
		/// </summary>
		public bool? AllowSubscriptions
		{
			get { return this.allowSubscriptions; }
			set { this.allowSubscriptions = value; }
		}

		/// <summary>
		/// A friendly name for the node
		/// </summary>
		public string Title
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// A description of the node
		/// </summary>
		public string[] Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		/// <summary>
		/// The type of node data, usually specified by the namespace of the payload (if any); MAY be list-single rather than text-single.
		/// </summary>
		public string PayloadType
		{
			get { return this.payloadType; }
			set { this.payloadType = value; }
		}

		/// <summary>
		/// Number of seconds after which to automatically purge items.
		/// </summary>
		public int? ItemExpireSeconds
		{
			get { return this.itemExpireSeconds; }
			set { this.itemExpireSeconds = value; }
		}

		/// <summary>
		/// Notification type
		/// </summary>
		public NotificationType? NotificationType
		{
			get { return this.notificationType; }
			set { this.notificationType = value; }
		}

		/// <summary>
		/// Purge all items when the relevant publisher goes offline
		/// </summary>
		public bool? PurgeOffline
		{
			get { return this.purgeOffline; }
			set { this.purgeOffline = value; }
		}

		/// <summary>
		/// Creates a data form for subsmission.
		/// </summary>
		/// <param name="Client">XMPP Publish/Subscribe Client</param>
		/// <returns>Data form.</returns>
		public DataForm ToForm(PubSubClient Client)
		{
			List<Field> Fields = new List<Field>()
			{
				new HiddenField("FORM_TYPE", "http://jabber.org/protocol/pubsub#node_config")
			};

			if (this.accessModel.HasValue)
				Fields.Add(new ListSingleField("pubsub#access_model", this.accessModel.Value.ToString()));

			if (!(this.bodyXsltUrl is null))
				Fields.Add(new TextSingleField("pubsub#body_xslt", this.bodyXsltUrl));

			if (!(this.collections is null))
				Fields.Add(new TextMultiField("pubsub#collection", this.collections));

			if (!(this.contact is null))
				Fields.Add(new TextMultiField("pubsub#contact", this.contact));

			if (!(this.dataFormXsltUrl is null))
				Fields.Add(new TextSingleField("pubsub#dataform_xslt", this.dataFormXsltUrl));

			if (this.deliverNotifications.HasValue)
				Fields.Add(new BooleanField("pubsub#deliver_notifications", this.deliverNotifications.Value));

			if (this.deliverPayloads.HasValue)
				Fields.Add(new BooleanField("pubsub#deliver_payloads", this.deliverPayloads.Value));

			if (this.itemReply.HasValue)
				Fields.Add(new ListSingleField("pubsub#itemreply", this.itemReply.Value.ToString()));

			if (this.childAssociationPolicy.HasValue)
				Fields.Add(new ListSingleField("pubsub#children_association_policy", this.childAssociationPolicy.Value.ToString()));

			if (!(this.childAssociationWhitelist is null))
				Fields.Add(new JidMultiField("pubsub#children_association_whitelist", this.childAssociationWhitelist));

			if (!(this.children is null))
				Fields.Add(new TextMultiField("pubsub#children", this.children));

			if (this.maxChildren.HasValue)
				Fields.Add(new TextSingleField("pubsub#children_max", this.maxChildren.Value.ToString()));

			if (this.maxItems.HasValue)
				Fields.Add(new TextSingleField("pubsub#max_items", this.maxItems.Value.ToString()));

			if (this.itemExpireSeconds.HasValue)
				Fields.Add(new TextSingleField("pubsub#item_expire", this.itemExpireSeconds.Value.ToString()));

			if (this.maxPayloadSize.HasValue)
				Fields.Add(new TextSingleField("pubsub#max_payload_size", this.maxPayloadSize.Value.ToString()));

			if (this.nodeType.HasValue)
				Fields.Add(new ListSingleField("pubsub#node_type", this.nodeType.Value.ToString()));

			if (this.notifyConfig.HasValue)
				Fields.Add(new BooleanField("pubsub#notify_config", this.notifyConfig.Value));

			if (this.notifyDelete.HasValue)
				Fields.Add(new BooleanField("pubsub#notify_delete", this.notifyDelete.Value));

			if (this.notifyRetract.HasValue)
				Fields.Add(new BooleanField("pubsub#notify_retract", this.notifyRetract.Value));

			if (this.notifySubscriptions.HasValue)
				Fields.Add(new BooleanField("pubsub#notify_sub", this.notifySubscriptions.Value));

			if (this.persistItems.HasValue)
				Fields.Add(new BooleanField("pubsub#persist_items", this.persistItems.Value));

			if (this.presenceBasedDelivery.HasValue)
				Fields.Add(new BooleanField("pubsub#presence_based_delivery", this.presenceBasedDelivery.Value));

			if (this.publisherModel.HasValue)
				Fields.Add(new ListSingleField("pubsub#publish_model", this.publisherModel.ToString()));

			if (!(this.replyRooms is null))
				Fields.Add(new JidMultiField("pubsub#replyroom", this.replyRooms));

			if (!(this.replyTo is null))
				Fields.Add(new JidMultiField("pubsub#replyto", this.replyTo));

			if (!(this.rosterGroupsAllowed is null))
				Fields.Add(new ListMultiField("pubsub#roster_groups_allowed", this.rosterGroupsAllowed));

			if (this.sendLastPublishedItem.HasValue)
				Fields.Add(new ListSingleField("pubsub#send_last_published_item", this.sendLastPublishedItem.ToString()));

			if (this.allowSubscriptions.HasValue)
				Fields.Add(new BooleanField("pubsub#subscribe", this.allowSubscriptions.Value));

			if (!(this.title is null))
				Fields.Add(new TextSingleField("pubsub#title", this.title));

			if (!(this.description is null))
				Fields.Add(new TextMultiField("pubsub#description", this.description));

			if (!(this.payloadType is null))
				Fields.Add(new TextSingleField("pubsub#type", this.payloadType));

			if (this.notificationType.HasValue)
				Fields.Add(new ListSingleField("pubsub#notification_type", this.notificationType.ToString()));

			if (this.purgeOffline.HasValue)
				Fields.Add(new BooleanField("pubsub#purge_offline", this.purgeOffline.Value));

			return new DataForm(Client.Client, FormType.Form, Client.Client.FullJID, 
				Client.ComponentAddress, Fields.ToArray());
		}

	}
}
