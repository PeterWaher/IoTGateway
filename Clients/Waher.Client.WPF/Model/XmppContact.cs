using System;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an XMPP contact whose capabilities have not been measured.
	/// </summary>
	public class XmppContact : TreeNode
	{
		private readonly XmppClient client;
		private readonly string bareJid;

		public XmppContact(TreeNode Parent, XmppClient Client, string BareJid)
			: base(Parent)
		{
			this.client = Client;
			this.bareJid = BareJid;
		}

		public override string Header
		{
			get { return this.bareJid; }
		}

		public override string TypeName
		{
			get { return "Unknown"; }
		}

		public string BareJID
		{
			get { return this.bareJid; }
		}

		public RosterItem RosterItem
		{
			get
			{
				return this.client[this.bareJid];
			}

			internal set
			{
				RosterItem Item = this.client[this.bareJid];
				if (Item != value)
					this.client[this.bareJid] = value;
			}
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
				if (AccountNode is null || !AccountNode.IsOnline)
					return Availability.Offline;

				RosterItem Item = this.client[this.bareJid];
				PresenceEventArgs e = Item?.LastPresence;

				if (e is null)
					return Availability.Offline;
				else
					return e.Availability;
			}
		}

		public SubscriptionState SubscriptionState
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
				if (AccountNode is null || !AccountNode.IsOnline)
					return SubscriptionState.Unknown;

				RosterItem Item = this.client[this.bareJid];
				if (Item is null)
					return SubscriptionState.Unknown;
				else
					return Item.State;
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

		public override ImageSource ImageResource2
		{
			get
			{
				switch (this.SubscriptionState)
				{
					case SubscriptionState.None:
						return XmppAccountNode.none;

					case SubscriptionState.From:
						return XmppAccountNode.from;

					case SubscriptionState.To:
						return XmppAccountNode.to;

					case SubscriptionState.Both:
						return XmppAccountNode.both;

					case SubscriptionState.Unknown:
					default:
						return null;
				}
			}
		}

		public override Visibility ImageResource2Visibility
		{
			get
			{
				switch (this.SubscriptionState)
				{
					case SubscriptionState.None:
					case SubscriptionState.From:
					case SubscriptionState.To:
					case SubscriptionState.Both:
						return Visibility.Visible;

					case SubscriptionState.Unknown:
					default:
						return Visibility.Hidden;
				}
			}
		}

		public override string ToolTip
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
				if (AccountNode is null)
					return string.Empty;
				else if (!AccountNode.IsOnline)
					return AccountNode.BareJID + " is not connected.";

				PresenceEventArgs e = this.RosterItem?.LastPresence;

				if (e is null)
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

		public override bool CanDelete
		{
			get { return true; }
		}

		public override bool CanEdit
		{
			get { return false; }   // TODO: Edit Friendly name, groups
		}

		public override bool CanRecycle
		{
			get { return false; }
		}

		public override string Key
		{
			get { return this.bareJid; }
		}

		public override bool CanChat
		{
			get
			{
				Availability Availability = this.Availability;

				return Availability != Availability.DoNotDisturb;
			}
		}

		public XmppAccountNode XmppAccountNode
		{
			get
			{
				TreeNode Loop = this.Parent;
				XmppAccountNode Result = Loop as XmppAccountNode;

				while (Result is null && Loop != null)
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
				string To = this.RosterItem?.LastPresenceFullJid;
				if (string.IsNullOrEmpty(To))
					To = this.bareJid;

				if (Markdown is null)
					XmppAccountNode.Client.SendChatMessage(To, Message);
				else
				{
					XmppAccountNode.Client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
						MultiFormatMessage(Markdown), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
				}
			}
		}

		public static string MultiFormatMessage(MarkdownDocument Markdown)
		{
			string PlainText = Markdown.GeneratePlainText().Trim();
			string HTML = HtmlDocument.GetBody(Markdown.GenerateHTML()).Trim();
			StringBuilder sb = new StringBuilder();

			sb.Append("<body>");
			sb.Append(XML.Encode(PlainText));
			sb.Append("</body>");
			sb.Append("<html xmlns='http://jabber.org/protocol/xhtml-im'>");
			sb.Append("<body xmlns='http://www.w3.org/1999/xhtml'>");
			sb.Append(HTML);
			sb.Append("</body></html>");
			sb.Append("<content xmlns='urn:xmpp:content' type='text/markdown'>");
			sb.Append(XML.HtmlValueEncode(Markdown.MarkdownText));
			sb.Append("</content>");

			return sb.ToString();
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			XmppAccountNode XmppAccountNode = this.XmppAccountNode;

			if (XmppAccountNode != null && XmppAccountNode.IsOnline)
			{
				SubscriptionState SubscriptionState = this.SubscriptionState;
				MenuItem MenuItem;
				string s;

				if (SubscriptionState == SubscriptionState.None || SubscriptionState == SubscriptionState.From)
				{
					CurrentGroup = "Subscriptions";

					if (SubscriptionState == SubscriptionState.None)
						s = "../Graphics/To.png";
					else
						s = "../Graphics/Both.png";

					Menu.Items.Add(MenuItem = new MenuItem()
					{
						Header = "_Subscribe to",
						IsEnabled = true,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri(s, UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					MenuItem.Click += Subscribe_Click;
				}

				if (SubscriptionState == SubscriptionState.To || SubscriptionState == SubscriptionState.Both)
				{
					CurrentGroup = "Subscriptions";

					if (SubscriptionState == SubscriptionState.To)
						s = "../Graphics/None.png";
					else
						s = "../Graphics/From.png";

					Menu.Items.Add(MenuItem = new MenuItem()
					{
						Header = "_Unsubscribe from",
						IsEnabled = true,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri(s, UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					MenuItem.Click += Unsubscribe_Click;
				}

			}
		}

		private void Subscribe_Click(object sender, RoutedEventArgs e)
		{
			this.XmppAccountNode?.Client?.RequestPresenceSubscription(this.bareJid);
		}

		private void Unsubscribe_Click(object sender, RoutedEventArgs e)
		{
			this.XmppAccountNode?.Client?.RequestPresenceUnsubscription(this.bareJid);
		}
	}
}
