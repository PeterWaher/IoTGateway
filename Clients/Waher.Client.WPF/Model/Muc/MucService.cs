using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Waher.Client.WPF.Dialogs.Muc;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Settings;

namespace Waher.Client.WPF.Model.Muc
{
	public class MucService : XmppComponent
	{
		private readonly MultiUserChatClient mucClient;
		private bool handlersAdded;

		public MucService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features,
			MultiUserChatClient MucClient)
			: base(Parent, JID, Name, Node, Features)
		{
			this.mucClient = MucClient;

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

			this.mucClient.OccupantPresence += MucClient_OccupantPresence;
			this.mucClient.OccupantRequest += MucClient_OccupantRequest;
			this.mucClient.PrivateMessageReceived += MucClient_PrivateMessageReceived;
			this.mucClient.RegistrationRequest += MucClient_RegistrationRequest;
			this.mucClient.RoomDeclinedInvitationReceived += MucClient_RoomDeclinedInvitationReceived;
			this.mucClient.RoomDestroyed += MucClient_RoomDestroyed;
			this.mucClient.RoomInvitationReceived += MucClient_RoomInvitationReceived;
			this.mucClient.RoomMessage += MucClient_RoomMessage;
			this.mucClient.RoomOccupantMessage += MucClient_RoomOccupantMessage;
			this.mucClient.RoomPresence += MucClient_RoomPresence;
			this.mucClient.RoomSubject += MucClient_RoomSubject;
			this.handlersAdded = true;
		}

		public override void Removed(MainWindow Window)
		{
			this.UnregisterHandlers();
		}

		public override void Dispose()
		{
			this.UnregisterHandlers();
			base.Dispose();
		}

		private void UnregisterHandlers()
		{
			if (this.handlersAdded)
			{
				this.mucClient.OccupantPresence -= MucClient_OccupantPresence;
				this.mucClient.OccupantRequest -= MucClient_OccupantRequest;
				this.mucClient.PrivateMessageReceived -= MucClient_PrivateMessageReceived;
				this.mucClient.RegistrationRequest -= MucClient_RegistrationRequest;
				this.mucClient.RoomDeclinedInvitationReceived -= MucClient_RoomDeclinedInvitationReceived;
				this.mucClient.RoomDestroyed -= MucClient_RoomDestroyed;
				this.mucClient.RoomInvitationReceived -= MucClient_RoomInvitationReceived;
				this.mucClient.RoomMessage -= MucClient_RoomMessage;
				this.mucClient.RoomOccupantMessage -= MucClient_RoomOccupantMessage;
				this.mucClient.RoomPresence -= MucClient_RoomPresence;
				this.mucClient.RoomSubject -= MucClient_RoomSubject;
				this.handlersAdded = false;
			}
		}

		public MultiUserChatClient MucClient
		{
			get { return this.mucClient; }
		}

		public override ImageSource ImageResource => XmppAccountNode.chatBubble;
		public override bool CanRecycle => true;

		public override void Recycle(MainWindow Window)
		{
			if (!(this.children is null))
			{
				foreach (TreeNode Node in this.children.Values)
				{
					if (Node.CanRecycle)
						Node.Recycle(Window);
				}
			}
		}

		public override string ToolTip
		{
			get
			{
				return "Multi-User Chat Service";
			}
		}

		private bool loadingChildren = false;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && !this.IsLoaded)
			{
				Mouse.OverrideCursor = Cursors.Wait;

				this.loadingChildren = true;
				this.Account.Client.SendServiceItemsDiscoveryRequest(this.mucClient.ComponentAddress, async (sender, e) =>
				{
					this.loadingChildren = false;
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

						this.NodesRemoved(this.children.Values, this);

						foreach (Item Item in e.Items)
						{
							string RoomId;
							string Domain;
							int i = Item.JID.IndexOf('@');

							if (i < 0)
							{
								RoomId = string.Empty;
								Domain = Item.JID;
							}
							else
							{
								RoomId = Item.JID.Substring(0, i);
								Domain = Item.JID.Substring(i + 1);
							}

							string NickName = await RuntimeSettings.GetAsync(RoomId + "@" + Domain + ".Nick", string.Empty);
							string Password = await RuntimeSettings.GetAsync(RoomId + "@" + Domain + ".Pwd", string.Empty);

							Children[Item.JID] = new RoomNode(this, RoomId, Domain, NickName, Password, Item.Name, false);
						}

						this.children = new SortedDictionary<string, TreeNode>(Children);
						this.OnUpdated();
						this.NodesAdded(Children.Values, this);
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get rooms." : e.ErrorText);

				}, null);
			}

			base.LoadChildren();
		}

		public override bool CanAddChildren => true;

		public override void Add()
		{
			EnterRoomForm Dialog = new EnterRoomForm()
			{
				Owner = MainWindow.currentInstance
			};

			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				string RoomId = Dialog.RoomID.Text;
				string Domain = this.mucClient.ComponentAddress;
				string NickName = Dialog.NickName.Text;
				string Password = Dialog.Password.Password;
				DataForm Form = null;

				this.mucClient.EnterRoom(RoomId, Domain, NickName, Password, async (sender, e) =>
				{
					if (e.Ok)
					{
						if (e.HasStatus(MucStatus.Created))
						{
							this.mucClient.GetRoomConfiguration(RoomId, Domain, (sender2, ConfigurationForm) =>
							{
								Form = ConfigurationForm;

								if (!string.IsNullOrEmpty(Password))
								{
									Field PasswordProtectionField = Form["muc#roomconfig_passwordprotectedroom"];
									PasswordProtectionField?.SetValue("true");

									Field PasswordField = Form["muc#roomconfig_roomsecret"];
									PasswordField?.SetValue(Password);
								}

								MainWindow.currentInstance.ShowDataForm(ConfigurationForm);
								return Task.CompletedTask;
							}, async (sender2, e2) =>
							{
								if (e2.Ok)
								{
									await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Nick", NickName);
									await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Pwd", Password);

									Field NameField = Form["muc#roomconfig_roomname"];
									string Name = NameField?.ValueString ?? RoomId;

									this.AddRoomNode(RoomId, Domain, NickName, Password, Name);
								}
							}, null);
						}
						else
						{
							await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Nick", NickName);
							await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Pwd", Password);

							this.AddRoomNode(RoomId, Domain, NickName, Password, RoomId);
						}
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to add room." : e.ErrorText);
				}, null);
			}
		}

		private void AddRoomNode(string RoomId, string Domain, string NickName, string Password, string Name)
		{
			if (this.IsLoaded)
			{
				RoomNode Node = new RoomNode(this, RoomId, Domain, NickName, Password, Name, true);

				if (this.children is null)
					this.children = new SortedDictionary<string, TreeNode>() { { Node.Key, Node } };
				else
				{
					lock (this.children)
					{
						this.children[Node.Key] = Node;
					}
				}

				MainWindow.UpdateGui(() =>
				{
					this.Account?.View?.NodeAdded(this, Node);

					foreach (TreeNode Node2 in Node.Children)
						this.Account?.View?.NodeAdded(Node, Node2);

					this.OnUpdated();
				});
			}
		}

		private Task MucClient_RoomInvitationReceived(object Sender, RoomInvitationMessageEventArgs e)
		{
			RoomInvitationReceivedForm Form = new RoomInvitationReceivedForm()
			{
				Owner = MainWindow.currentInstance,
				InviteFrom = e.InviteFrom,
				Password = e.Password,
				InvitationReason = e.Reason,
				RoomId = e.RoomId,
				RoomName = e.RoomId,
				Domain = e.Domain,
				RoomJid = e.RoomId + "@" + e.Domain
			};

			this.mucClient.GetRoomInformation(e.RoomId, e.Domain, (sender2, e2) =>
			{
				if (e2.Ok)
				{
					foreach (Identity Id in e2.Identities)
					{
						if (Id.Category == "conference" && Id.Type == "text")
						{
							Form.RoomName = Id.Name;
							break;
						}
					}

					Form.MembersOnly = e2.MembersOnly;
					Form.Moderated = e2.Moderated;
					Form.NonAnonymous = e2.NonAnonymous;
					Form.Open = e2.Open;
					Form.PasswordProtected = e2.PasswordProtected;
					Form.Persistent = e2.Persistent;
					Form.Public = e2.Public;
					Form.SemiAnonymous = e2.SemiAnonymous;
					Form.Temporary = e2.Temporary;
					Form.Unmoderated = e2.Unmoderated;
					Form.Unsecured = e2.Unsecured;
				}

				MainWindow.UpdateGui(() =>
				{
					bool? b = Form.ShowDialog();

					if (b.HasValue)
					{
						if (b.Value)
							e.Accept(Form.NickName.Text);
						else
							e.Decline(Form.Reason.Text);
					}
				});

				return Task.CompletedTask;
			}, null);

			return Task.CompletedTask;
		}

		private Task MucClient_RoomDeclinedInvitationReceived(object Sender, RoomDeclinedMessageEventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Your invitation to join the room ");
			sb.Append(e.RoomId);
			sb.Append("@");
			sb.Append(e.Domain);
			sb.Append(" sent to ");
			sb.Append(e.DeclinedFrom);
			sb.Append(" was declined.");

			if (!string.IsNullOrEmpty(e.Reason))
			{
				sb.Append(" Reason: ");
				sb.Append(e.Reason);
			}

			MainWindow.MessageBox(sb.ToString(), "Invitation declined", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			
			return Task.CompletedTask;
		}

		private Task MucClient_RoomSubject(object Sender, RoomOccupantMessageEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_RoomPresence(object Sender, UserPresenceEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_RoomOccupantMessage(object Sender, RoomOccupantMessageEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_RoomMessage(object Sender, RoomMessageEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_RoomDestroyed(object Sender, UserPresenceEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_RegistrationRequest(object Sender, Networking.XMPP.MessageFormEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_PrivateMessageReceived(object Sender, RoomOccupantMessageEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_OccupantRequest(object Sender, Networking.XMPP.MessageFormEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

		private Task MucClient_OccupantPresence(object Sender, UserPresenceEventArgs e)
		{
			// TODO
			return Task.CompletedTask;
		}

	}
}
