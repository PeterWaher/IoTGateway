using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an unspecialized XMPP contact.
	/// </summary>
	public class XmppContact : TreeNode
	{
		private RosterItem rosterItem;

		public XmppContact(TreeNode Parent, RosterItem RosterItem)
			: base(Parent)
		{
			this.rosterItem = RosterItem;
		}

		public override string Header
		{
			get { return this.rosterItem.BareJid; }
		}

		public string BareJID
		{
			get { return this.rosterItem.BareJid; }
		}

		public RosterItem RosterItem
		{
			get { return this.rosterItem; }
			internal set { this.rosterItem = value; }
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		public override ImageSource ImageResource
		{
			get
			{
				XmppAccountNode AccountNode = this.Parent as XmppAccountNode;
				if (AccountNode == null || !AccountNode.IsOnline)
					return XmppAccountNode.offline;

				PresenceEventArgs e = this.rosterItem.LastPresence;

				if (e == null)
					return XmppAccountNode.offline;
				else
				{
					switch (e.Availability)
					{
						case Availability.Away:
							return XmppAccountNode.away;

						case Availability.Chat:
							return XmppAccountNode.chat;

						case Availability.DoNotDisturb:
							return XmppAccountNode.busy;

						case Availability.ExtendedAway:
							return XmppAccountNode.away;	// TODO: Add icon

						case Availability.Online:
							return XmppAccountNode.online;

						case Availability.Offline:
						default:
							return XmppAccountNode.offline;
					}
				}
			}
		}

		public override string ToolTip
		{
			get
			{
				XmppAccountNode AccountNode = this.Parent as XmppAccountNode;
				if (AccountNode == null)
					return string.Empty;
				else if (!AccountNode.IsOnline)
					return AccountNode.BareJID + " is not connected.";

				PresenceEventArgs e = this.rosterItem.LastPresence;

				if (e == null)
					return "Status unknown. No presence received.";
				else
				{
					switch (e.Availability)
					{
						case Availability.Away:
							return "Contact is away.";

						case Availability.Chat:
							return "Contact is ready to chat.";

						case Availability.DoNotDisturb:
							return "Contact is busy and doesn't want to be disturbed.";

						case Availability.ExtendedAway:
							return "Contact is away for an extended period.";

						case Availability.Online:
							return "Contact is online.";

						case Availability.Offline:
						default:
							return "Contact is offline.";
					}
				}
			}
		}

		public override bool CanAddChildren
		{
			get { return false; }
		}

		public override bool CanRecycle
		{
			get { return false; }
		}

		public override string Key
		{
			get { return this.rosterItem.BareJid; }
		}
	}
}
