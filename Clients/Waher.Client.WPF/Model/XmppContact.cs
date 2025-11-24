using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Client.WPF.Controls;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.RDP;
using Waher.Networking.XMPP.Events;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an XMPP contact whose capabilities have not been measured.
	/// </summary>
	public class XmppContact(TreeNode Parent, XmppClient Client, string BareJid, bool SupportsRdp) : XmppNode(Parent)
	{
		private readonly XmppClient client = Client;
		private readonly string bareJid = BareJid;
		private readonly bool supportsRdp = SupportsRdp;

		public override string Header => this.bareJid;

		public override string TypeName
		{
			get { return "Unknown"; }
		}

		public string BareJID => this.bareJid;

		public override string FullJID
		{
			get
			{
				RosterItem Item = this.client[this.bareJid];
				if (Item is null || !Item.HasLastPresence)
					return this.bareJid;
				else
					return Item.LastPresenceFullJid;
			}
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
				return this.Availability switch
				{
					Availability.Away => XmppAccountNode.away,
					Availability.Chat => XmppAccountNode.chat,
					Availability.DoNotDisturb => XmppAccountNode.busy,
					Availability.ExtendedAway => XmppAccountNode.extendedAway,
					Availability.Online => XmppAccountNode.online,
					_ => XmppAccountNode.offline,
				};
			}
		}

		public override ImageSource ImageResource2
		{
			get
			{
				return this.SubscriptionState switch
				{
					SubscriptionState.None => XmppAccountNode.none,
					SubscriptionState.From => XmppAccountNode.from,
					SubscriptionState.To => XmppAccountNode.to,
					SubscriptionState.Both => XmppAccountNode.both,
					_ => null,
				};
			}
		}

		public override Visibility ImageResource2Visibility
		{
			get
			{
				return this.SubscriptionState switch
				{
					SubscriptionState.None or SubscriptionState.From or SubscriptionState.To or SubscriptionState.Both => Visibility.Visible,
					_ => Visibility.Hidden,
				};
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
					return e.Availability switch
					{
						Availability.Away => "Contact is away.",
						Availability.Chat => "Contact is ready to chat.",
						Availability.DoNotDisturb => "Contact is busy and doesn't want to be disturbed.",
						Availability.ExtendedAway => "Contact is away for an extended period.",
						Availability.Online => "Contact is online.",
						_ => "Contact is offline.",
					};
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

		public override string Key => this.bareJid;

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

				while (Result is null && Loop is not null)
				{
					Loop = Loop.Parent;
					Result = Loop as XmppAccountNode;
				}

				return Result;
			}
		}

		public override async Task SendChatMessage(string Message, string ThreadId, MarkdownDocument Markdown)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			if (XmppAccountNode is not null)
			{
				string To = this.RosterItem?.LastPresenceFullJid;
				if (string.IsNullOrEmpty(To))
					To = this.bareJid;

				if (Markdown is null)
					await XmppAccountNode.Client.SendChatMessage(To, Message, string.Empty, string.Empty, ThreadId);
				else
				{
					await XmppAccountNode.Client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
						await MultiFormatMessage(Message, Markdown), string.Empty, string.Empty, string.Empty, ThreadId, string.Empty, null, null);
				}
			}
		}

		public static async Task<string> MultiFormatMessage(string PlainText, MarkdownDocument Markdown)
		{
			PlainText ??= (await Markdown.GeneratePlainText()).Trim();

			string HTML = HtmlDocument.GetBody(await Markdown.GenerateHTML()).Trim();
			StringBuilder sb = new();

			if (!string.IsNullOrEmpty(PlainText))
			{
				sb.Append("<body>");
				sb.Append(XML.Encode(PlainText));
				sb.Append("</body>");
			}

			sb.Append("<html xmlns='http://jabber.org/protocol/xhtml-im'>");
			sb.Append("<body xmlns='http://www.w3.org/1999/xhtml'>");
			sb.Append(HTML);
			sb.Append("</body></html>");
			sb.Append("<content xmlns='urn:xmpp:content' type='text/markdown'>");
			sb.Append(XML.HtmlValueEncode(await Markdown.GenerateMarkdown()));
			sb.Append("</content>");

			return sb.ToString();
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			XmppAccountNode XmppAccountNode = this.XmppAccountNode;

			if (XmppAccountNode is not null && XmppAccountNode.IsOnline)
			{
				SubscriptionState SubscriptionState = this.SubscriptionState;
				MenuItem MenuItem;
				string s;

				if (SubscriptionState == SubscriptionState.None || SubscriptionState == SubscriptionState.From)
				{
					GroupSeparator(ref CurrentGroup, "Subscriptions", Menu);

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

					MenuItem.Click += this.Subscribe_Click;
				}

				if (SubscriptionState == SubscriptionState.To || SubscriptionState == SubscriptionState.Both)
				{
					GroupSeparator(ref CurrentGroup, "Subscriptions", Menu);

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

					MenuItem.Click += this.Unsubscribe_Click;
				}

			}
		}

		private void Subscribe_Click(object Sender, RoutedEventArgs e)
		{
			this.XmppAccountNode?.Client?.RequestPresenceSubscription(this.bareJid);
		}

		private void Unsubscribe_Click(object Sender, RoutedEventArgs e)
		{
			this.XmppAccountNode?.Client?.RequestPresenceUnsubscription(this.bareJid);
		}

		public override bool CanConfigure => this.supportsRdp && !string.IsNullOrEmpty(this.LastOnlineFullJid);

		private string LastOnlineFullJid
		{
			get
			{
				try
				{
					if (this.client is null)
						return null;

					RosterItem Item = this.client[this.bareJid];
					if (Item is null)
						return null;

					PresenceEventArgs e = Item.LastPresence;
					if (e?.IsOnline ?? false)
						return e.From;
					else
						return null;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// If actuator control should be used (instead of RDP control)
		/// </summary>
		protected virtual bool UseActuatorControl => false;

		public override async void Configure()
		{
			if (this.UseActuatorControl)
			{
				base.Configure();
				return;
			}

			string FullJid = this.LastOnlineFullJid;
			if (string.IsNullOrEmpty(FullJid))
				return;

			RemoteDesktopClient RdpClient = this.XmppAccountNode?.RdpClient;
			if (RdpClient is null)
				return;

			XmppClient Client = this.client;
			bool DisposeRdpClient = false;

			try
			{
				Mouse.OverrideCursor = Cursors.Wait;

				Guid SessionGuid = Guid.NewGuid();
				RemoteDesktopView View = new(this, Client, RdpClient, DisposeRdpClient);

				this.XmppAccountNode?.ReregisterView(SessionGuid.ToString(), View);

				View.Session = await RdpClient.StartSessionAsync(FullJid, SessionGuid);

				Mouse.OverrideCursor = null;

				TabItem TabItem = MainWindow.NewTab(this.bareJid);
				MainWindow.currentInstance!.Tabs.Items.Add(TabItem);

				TabItem.Content = View;

				MainWindow.currentInstance.Tabs.SelectedItem = TabItem;
			}
			catch (Exception ex)
			{
				if (DisposeRdpClient)
					RdpClient.Dispose();

				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// If node can be copied to clipboard.
		/// </summary>
		public override bool CanCopy => true;

		/// <summary>
		/// Is called when the user wants to copy the node to the clipboard.
		/// </summary>
		public override void Copy()
		{
			Clipboard.SetText("xmpp:" + this.bareJid);
		}
	}
}
