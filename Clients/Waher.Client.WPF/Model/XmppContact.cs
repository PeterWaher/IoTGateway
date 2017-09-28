using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an XMPP contact whose capabilities have not been measured.
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

		public override string TypeName
		{
			get { return "Unknown"; }
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

		public Availability Availability
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
				if (AccountNode == null || !AccountNode.IsOnline)
					return Availability.Offline;

				PresenceEventArgs e = this.rosterItem.LastPresence;

				if (e == null)
					return Availability.Offline;
				else
					return e.Availability;
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				switch (this.Availability)
				{
					case Availability.Away:
						return XmppAccountNode.away;

					case Availability.Chat:
						return XmppAccountNode.chat;

					case Availability.DoNotDisturb:
						return XmppAccountNode.busy;

					case Availability.ExtendedAway:
						return XmppAccountNode.extendedAway;

					case Availability.Online:
						return XmppAccountNode.online;

					case Availability.Offline:
					default:
						return XmppAccountNode.offline;
				}
			}
		}

		public override string ToolTip
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
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

		public override bool CanChat
		{
			get
			{
				return this.Availability != Availability.Offline;
			}
		}

		public XmppAccountNode XmppAccountNode
		{
			get
			{
				TreeNode Loop = this.Parent;
				XmppAccountNode Result = Loop as XmppAccountNode;

				while (Result == null && Loop != null)
				{
					Loop = Loop.Parent;
					Result = Loop as XmppAccountNode;
				}

				return Result;
			}
		}

		public override void SendChatMessage(string Message, MarkdownDocument Markdown)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			if (XmppAccountNode != null)
			{
				if (Markdown == null)
					XmppAccountNode.Client.SendChatMessage(this.rosterItem.LastPresenceFullJid, Message);
				else
				{
					string PlainText = Markdown.GeneratePlainText();

					XmppAccountNode.Client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, this.rosterItem.LastPresenceFullJid,
						"<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">" + XML.Encode(Message) + "</content>", PlainText,
						string.Empty, string.Empty, string.Empty, string.Empty, null, null);
				}
			}
		}

	}
}
