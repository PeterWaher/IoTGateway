using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// State of a presence subscription.
	/// </summary>
	public enum SubscriptionState
	{
		/// <summary>
		/// The user does not have a subscription to the contact's presence, and the contact does not have a subscription to the
		/// user's presence; this is the default value, so if the subscription attribute is not included then the state is to be understood as
		/// "none".
		/// </summary>
		None,

		/// <summary>
		/// The user has a subscription to the contact's presence, but the contact does not have a subscription to the user's presence.
		/// </summary>
		To,

		/// <summary>
		/// The contact has a subscription to the user's presence, but the user does not have a subscription to the contact's presence.
		/// </summary>
		From,

		/// <summary>
		/// The user and the contact have subscriptions to each other's presence (also called a "mutual subscription").
		/// </summary>
		Both,

		/// <summary>
		/// Roster item is to be removed, or has been removed. Only available in roster push messages.
		/// </summary>
		Remove,

		/// <summary>
		/// Unknown subscription state.
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Pending subscription states.
	/// </summary>
	public enum PendingSubscription
	{
		/// <summary>
		/// Pending subscribe
		/// </summary>
		Subscribe,

		/// <summary>
		/// Pending unsubscribe
		/// </summary>
		Unsubscribe,

		/// <summary>
		/// No pending subscriptions
		/// </summary>
		None
	}

	/// <summary>
	/// Maintains information about an item in the roster.
	/// </summary>
	public class RosterItem
	{
		private readonly Dictionary<string, PresenceEventArgs> resources;
		private string[] groups;
		private SubscriptionState state;
		private PendingSubscription pendingSubscription;
		private PresenceEventArgs lastPresence = null;
		private string bareJid;
		private string name;

		/// <summary>
		/// Maintains information about an item in the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <param name="Name">Name of the roster item.</param>
		/// <param name="Groups">Groups assigned to the roster item.</param>
		public RosterItem(string BareJID, string Name, params string[] Groups)
			: this(BareJID, Name, Groups, null)
		{
		}

		/// <summary>
		/// Maintains information about an item in the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <param name="Name">Name of the roster item.</param>
		/// <param name="Groups">Groups assigned to the roster item.</param>
		/// <param name="Prev">Inherit resources from the previous roster item.</param>
		internal RosterItem(string BareJID, string Name, string[] Groups, RosterItem Prev)
		{
			this.groups = Groups;
			this.state = SubscriptionState.Unknown;
			this.bareJid = BareJID;
			this.name = Name;
			this.pendingSubscription = PendingSubscription.None;
			this.resources = Prev?.resources ?? new Dictionary<string, PresenceEventArgs>();
		}

		internal RosterItem(XmlElement Item, Dictionary<string, RosterItem> Roster)
		{
			this.bareJid = XML.Attribute(Item, "jid");
			this.name = XML.Attribute(Item, "name");

			switch (XML.Attribute(Item, "subscription"))
			{
				case "both":
					this.state = SubscriptionState.Both;
					break;

				case "to":
					this.state = SubscriptionState.To;
					break;

				case "from":
					this.state = SubscriptionState.From;
					break;

				case "none":
				case "":
					this.state = SubscriptionState.None;
					break;

				case "remove":
					this.state = SubscriptionState.Remove;
					break;

				default:
					this.state = SubscriptionState.Unknown;
					break;
			}

			switch (XML.Attribute(Item, "ask").ToLower())
			{
				case "subscribe":
					this.pendingSubscription = PendingSubscription.Subscribe;
					break;

				case "unsubscribe":
					this.pendingSubscription = PendingSubscription.Unsubscribe;
					break;

				default:
					this.pendingSubscription = PendingSubscription.None;
					break;
			}

			List<string> Groups = new List<string>();

			foreach (XmlNode N in Item.ChildNodes)
			{
				if (N.LocalName == "group")
					Groups.Add(N.InnerText);
			}

			this.groups = Groups.ToArray();

			if (this.state == SubscriptionState.Both || this.state == SubscriptionState.To)
			{
				if (Roster.TryGetValue(this.bareJid, out RosterItem Prev))
					this.resources = Prev.resources;

				this.lastPresence = Prev?.lastPresence;
			}

			if (this.resources == null)
				this.resources = new Dictionary<string, PresenceEventArgs>();
		}

		/// <summary>
		/// Any groups the roster item belongs to.
		/// </summary>
		public string[] Groups
		{
			get { return this.groups; }
			internal set { this.groups = value; }
		}

		/// <summary>
		/// Checks if the roster item is in a specific group.
		/// </summary>
		/// <param name="Group">Name of group.</param>
		/// <returns>If the item is in the group.</returns>
		public bool IsInGroup(string Group)
		{
			return Array.IndexOf<string>(this.groups, Group) >= 0;
		}

		/// <summary>
		/// Current subscription state.
		/// </summary>
		public SubscriptionState State
		{
			get { return this.state; }
			internal set { this.state = value; }
		}

		/// <summary>
		/// Bare JID of the roster item.
		/// </summary>
		public string BareJid
		{
			get { return this.bareJid; }
		}

		/// <summary>
		/// Name of the roster item.
		/// </summary>
		public string Name
		{
			get { return this.name; }
			internal set { this.name = value; }
		}

		/// <summary>
		/// If there's a pending unanswered presence subscription or unsubscription request made to the contact.
		/// </summary>
		public PendingSubscription PendingSubscription
		{
			get { return this.pendingSubscription; }
			internal set { this.pendingSubscription = value; }
		}

		/// <summary>
		/// Active resources utilized by contact.
		/// </summary>
		public PresenceEventArgs[] Resources
		{
			get
			{
				lock (this.resources)
				{
					PresenceEventArgs[] Result = new PresenceEventArgs[this.resources.Count];
					this.resources.Values.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// Serializes the roster item for transmission to the server.
		/// </summary>
		/// <param name="Output">Output</param>
		public void Serialize(StringBuilder Output)
		{
			Output.Append("<item jid='");
			Output.Append(XML.Encode(this.bareJid));

			Output.Append("' name='");
			Output.Append(XML.Encode(this.name));

			if (this.state == SubscriptionState.Remove)
				Output.Append("' subscription='remove");

			Output.Append("'>");

			foreach (string Group in this.groups)
			{
				Output.Append("<group>");
				Output.Append(XML.Encode(Group));
				Output.Append("</group>");
			}

			Output.Append("</item>");
		}

		/// <summary>
		/// Full JID of last resource sending online presence.
		/// </summary>
		public string LastPresenceFullJid
		{
			get
			{
				if (this.lastPresence != null)
					return this.lastPresence.From;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Last presence received from a resource having this bare JID.
		/// </summary>
		public PresenceEventArgs LastPresence
		{
			get { return this.lastPresence; }
		}

		internal void PresenceReceived(XmppClient Client, PresenceEventArgs e)
		{
			PresenceEventArgs[] ToTest = null;

			lock (this.resources)
			{
				if (e.Type == PresenceType.Unavailable)
				{
					this.resources.Remove(e.From);

					if (this.lastPresence != null && this.lastPresence.From == e.From)
						this.lastPresence = null;
				}
				else if (e.Type == PresenceType.Available)
				{
					int c = this.resources.Count;

					if (c > 0 && Client.MonitorContactResourcesAlive && !this.resources.ContainsKey(e.From))
					{
						ToTest = new PresenceEventArgs[c];
						this.resources.Values.CopyTo(ToTest, 0);
					}

					this.resources[e.From] = e;
					this.lastPresence = e;

					if (this.pendingSubscription == PendingSubscription.Subscribe)
						this.pendingSubscription = PendingSubscription.None;    // Might be out of synch.
				}
			}

			if (ToTest != null)
			{
				foreach (PresenceEventArgs e2 in ToTest)
					Client.SendPing(e2.From, this.PingResult, new object[] { Client, e2 });
			}
		}

		private void PingResult(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
				return;

			if (e.ErrorElement != null && e.ErrorElement.LocalName == FeatureNotImplementedException.LocalName)
				return;

			object[] P = (object[])e.State;
			XmppClient Client = (XmppClient)P[0];
			PresenceEventArgs e2 = (PresenceEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(e2.From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(Client.FullJID));
			Xml.Append("' type='unavailable'/>");

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml.ToString());

			Client.ProcessPresence(new PresenceEventArgs(Client, Doc.DocumentElement));
		}

		/// <summary>
		/// If the roster item has received presence from an online resource having the given bare JID.
		/// </summary>
		public bool HasLastPresence
		{
			get
			{
				return this.lastPresence != null;
			}
		}

		/// <summary>
		/// Returns the name of the contact, or the Bare JID, if there's no name provided.
		/// </summary>
		public string NameOrBareJid
		{
			get
			{
				if (string.IsNullOrEmpty(this.name))
					return this.bareJid;
				else
					return this.name;
			}
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is RosterItem Item))
				return false;

			int i, c;

			if (this.state != Item.state ||
				this.pendingSubscription != Item.pendingSubscription ||
				this.bareJid != Item.bareJid ||
				this.name != Item.bareJid ||
				(c = this.groups.Length) != Item.groups.Length)
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (this.groups[i] != Item.groups[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.state.GetHashCode();
			Result *= 33;
			Result |= this.pendingSubscription.GetHashCode();
			Result *= 33;
			Result |= this.bareJid.GetHashCode();
			Result *= 33;
			Result |= this.name.GetHashCode();

			foreach (string Group in this.groups)
			{
				Result *= 33;
				Result |= Group.GetHashCode();
			}

			return Result;
		}

	}
}
