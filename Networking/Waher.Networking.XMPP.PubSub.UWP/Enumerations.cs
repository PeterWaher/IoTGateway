using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Node access model.
	/// </summary>
	public enum NodeAccessModel
	{
		/// <summary>
		/// The node owner must approve all subscription requests, and only subscribers may retrieve items from the node.
		/// </summary>
		authorize,

		/// <summary>
		/// Any entity may subscribe to the node (i.e., without the necessity for subscription approval) and any entity may retrieve 
		/// items from the node (i.e., without being subscribed); this SHOULD be the default access model for generic pubsub services.
		/// </summary>
		open,

		/// <summary>
		/// Any entity with a subscription of type "from" or "both" may subscribe to the node and retrieve items from the node; this 
		/// access model applies mainly to instant messaging systems (see RFC 3921).
		/// </summary>
		presence,

		/// <summary>
		/// Any entity in the specified roster group(s) may subscribe to the node and retrieve items from the node; this access model applies 
		/// mainly to instant messaging systems (see RFC 3921).
		/// </summary>
		roster,

		/// <summary>
		/// An entity may subscribe or retrieve items only if on a whitelist managed by the node owner. The node owner MUST automatically be on 
		/// the whitelist. In order to add entities to the whitelist, the node owner SHOULD use the protocol specified in the Manage Affiliated 
		/// Entities section of this document, specifically by setting the affiliation to "member".
		/// </summary>
		whitelist
	}

	/// <summary>
	/// Publisher model
	/// </summary>
	public enum PublisherModel
	{
		/// <summary>
		/// Only publishers may publish
		/// </summary>
		publishers,

		/// <summary>
		/// Subscribers may publish
		/// </summary>
		subscribers,

		/// <summary>
		/// Anyone may publish
		/// </summary>
		open
	}

	/// <summary>
	/// When to send the last published item	
	/// </summary>
	public enum SendLastPublishedItem
	{
		/// <summary>
		/// Never
		/// </summary>
		never,

		/// <summary>
		/// When a new subscription is processed
		/// </summary>
		on_sub,

		/// <summary>
		/// When a new subscription is processed and whenever a subscriber comes online
		/// </summary>
		on_sub_and_presence
	}

	/// <summary>
	/// Delivery style for notifications
	/// </summary>
	public enum NotificationType
	{
		/// <summary>
		/// Messages of type normal
		/// </summary>
		normal,

		/// <summary>
		/// Messages of type headline
		/// </summary>
		headline
	}

	/// <summary>
	/// Whether owners or publisher should receive replies to items
	/// </summary>
	public enum NodeItemReply
	{
		/// <summary>
		/// Statically specify a replyto of the node owner(s)
		/// </summary>
		owner,

		/// <summary>
		/// Dynamically specify a replyto of the item publisher
		/// </summary>
		publisher
	}

	/// <summary>
	/// Who may associate leaf nodes with a collection
	/// </summary>
	public enum NodeChildAssociationPolicy
	{
		/// <summary>
		/// Anyone may associate leaf nodes with the collection
		/// </summary>
		all,

		/// <summary>
		/// Only collection node owners may associate leaf nodes with the collection
		/// </summary>
		owners,

		/// <summary>
		/// Only those on a whitelist may associate leaf nodes with the collection
		/// </summary>
		whitelist
	}

	/// <summary>
	/// Whether the node is a leaf (default) or a collection
	/// </summary>
	public enum NodeType
	{
		/// <summary>
		/// The node is a leaf node (default)
		/// </summary>
		leaf,

		/// <summary>
		/// The node is a collection node
		/// </summary>
		collection
	}

	/// <summary>
	/// Node subscription status
	/// </summary>
	public enum NodeSubscriptionStatus
	{
		/// <summary>
		/// No subscription active
		/// </summary>
		none,

		/// <summary>
		/// Subscription pending approval
		/// </summary>
		pending,

		/// <summary>
		/// Subscribed
		/// </summary>
		subscribed,

		/// <summary>
		/// Subscription requires configuration
		/// </summary>
		unconfigured
	}

	/// <summary>
	/// Subscription type
	/// </summary>
	public enum SubscriptionType
	{
		/// <summary>
		/// Receive notification of new items only
		/// </summary>
		items,

		/// <summary>
		/// Receive notification of new nodes only
		/// </summary>
		nodes
	}

	/// <summary>
	/// Subscription depth
	/// </summary>
	public enum SubscriptonDepth
	{
		/// <summary>
		/// Receive notification from direct child nodes only
		/// </summary>
		one,

		/// <summary>
		/// Receive notification from all descendent nodes
		/// </summary>
		all
	}

	/// <summary>
	/// Availability of options.
	/// </summary>
	public enum OptionsAvailability
	{
		/// <summary>
		/// Support for options unknown.
		/// </summary>
		Unknown,

		/// <summary>
		/// Options supported.
		/// </summary>
		Supported,

		/// <summary>
		/// Options required.
		/// </summary>
		Required
	}

	/// <summary>
	/// User affiliation
	/// </summary>
	public enum AffiliationStatus
	{
		/// <summary>
		/// Owners can: Subscribe, Retrieve Items, Publish Items, Delete Single Item, Purge Node, Configure Node, Delete Node
		/// </summary>
		owner,

		/// <summary>
		/// Publishers can: Subscribe, Retrieve Items, Publish Items, Delete Single Item, Purge Node
		/// </summary>
		publisher,

		/// <summary>
		/// Publish-only can: Publish Items, Delete Single Item (that they have published), Purge Node (of items that they have published)
		/// </summary>
		publishOnly,

		/// <summary>
		/// Members can: Subscribe, Retrieve Items
		/// </summary>
		member,

		/// <summary>
		/// Members can: Subscribe
		/// </summary>
		none,

		/// <summary>
		/// Outcast can do nothing.
		/// </summary>
		outcast
	}
}
