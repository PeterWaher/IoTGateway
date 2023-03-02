using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Xmpp.Model;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Node representing a roster item in the roster of an XMPP account.
	/// </summary>
	public class RosterItemNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Node representing a roster item in the roster of an XMPP account.
		/// </summary>
		public RosterItemNode()
			: base()
		{
		}

		/// <summary>
		/// Partition ID
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(29, "Bare JID:")]
		[ToolTip(30, "Bare JID of roster item.")]
		public string BareJID { get; set; }

		/// <summary>
		/// Subscription State
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(34, "Subscription State:")]
		[ToolTip(35, "State of presence subscription for roster item.")]
		[Option(SubscriptionState.None, 36, "None")]
		[Option(SubscriptionState.To, 37, "To")]
		[Option(SubscriptionState.From, 38, "From")]
		[Option(SubscriptionState.Both, 39, "Both")]
		[Option(SubscriptionState.Remove, 40, "Remove")]
		[Option(SubscriptionState.Unknown, 41, "Unknown")]
		public SubscriptionState SubscriptionState { get; set; }

		/// <summary>
		/// Partition ID
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(42, "Name:")]
		[ToolTip(43, "Name of contact.")]
		public string ContactName { get; set; }

		/// <summary>
		/// Subscription State
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(44, "Pending Subscription:")]
		[ToolTip(47, "If a pending subscription request exists.")]
		[Option(PendingSubscription.None, 36, "None")]
		[Option(PendingSubscription.Subscribe, 45, "Subscribe")]
		[Option(PendingSubscription.Unsubscribe, 46, "Unsubscribe")]
		public PendingSubscription PendingSubscription { get; set; }

		/// <summary>
		/// Subscription State
		/// </summary>
		[Page(28, "Roster", 110)]
		[Header(48, "Groups:")]
		[ToolTip(49, "Groups to which the roster item belongs.")]
		public string[] Groups { get; set; }

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public override string LocalId => this.BareJID;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 31, "Roster Item");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is XmppBrokerNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
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

			if (this.Parent is XmppBrokerNode BrokerNode)
			{
				XmppBroker Broker = BrokerNode.GetBroker();
				XmppClient Client = Broker.Client;
				Namespace Namespace = await Language.GetNamespaceAsync(typeof(XmppBrokerNode).Namespace);

				RosterItem Item = Client[this.BareJID];
				if (!(Item is null))
					Result.AddLast(new StringParameter("SubscriptionState", await Namespace.GetStringAsync(51, "Subscription State"), Item.State.ToString()));
				
				Result.AddLast(new StringParameter("ContactName", await Namespace.GetStringAsync(50, "Contact Name"), this.ContactName));
			}

			return Result;
		}

	}
}
