using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Dialogs.Muc;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Settings;

namespace Waher.Client.WPF.Model.Muc
{
	public class MucService : XmppComponent
	{
		private readonly Dictionary<string, RoomNode> roomByJid = new Dictionary<string, RoomNode>();
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

		public override bool RemoveChild(TreeNode Node)
		{
			if (Node is RoomNode RoomNode)
			{
				lock (this.roomByJid)
				{
					this.roomByJid.Remove(RoomNode.RoomId + "@" + RoomNode.Domain);
				}
			}

			return base.RemoveChild(Node);
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

						lock (this.roomByJid)
						{
							foreach (KeyValuePair<string, RoomNode> P in this.roomByJid)
								Children[P.Key] = P.Value;
						}

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

							string Jid = RoomId + "@" + Domain;

							string Prefix = this.mucClient.Client.BareJID + "." + Jid;
							string NickName = await RuntimeSettings.GetAsync(Prefix + ".Nick", string.Empty);
							string Password = await RuntimeSettings.GetAsync(Prefix + ".Pwd", string.Empty);

							lock (this.roomByJid)
							{
								if (!this.roomByJid.TryGetValue(Jid, out RoomNode Node))
								{
									Node = new RoomNode(this, RoomId, Domain, NickName, Password, Item.Name, false);
									this.roomByJid[Jid] = Node;
									Children[Item.JID] = Node;
								}
							}
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
			EnterRoomForm Dialog = new EnterRoomForm(this.mucClient.ComponentAddress)
			{
				Owner = MainWindow.currentInstance
			};

			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				string RoomId = Dialog.RoomID.Text;
				string Domain = Dialog.Domain.Text;
				string NickName = Dialog.NickName.Text;
				string Password = Dialog.Password.Password;
				string Prefix = this.mucClient.Client.BareJID + "." + RoomId + "@" + Domain;
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
									await RuntimeSettings.SetAsync(Prefix + ".Nick", NickName);
									await RuntimeSettings.SetAsync(Prefix + ".Pwd", Password);

									Field NameField = Form["muc#roomconfig_roomname"];
									string Name = NameField?.ValueString ?? RoomId;

									this.AddRoomNode(RoomId, Domain, NickName, Password, Name, true);
								}
							}, null);
						}
						else
						{
							await RuntimeSettings.SetAsync(Prefix + ".Nick", NickName);
							await RuntimeSettings.SetAsync(Prefix + ".Pwd", Password);

							this.AddRoomNode(RoomId, Domain, NickName, Password, string.Empty, true);

							await this.MucClient_OccupantPresence(this, e);
						}
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to add room." : e.ErrorText);
				}, null);
			}
		}

		private RoomNode AddRoomNode(string RoomId, string Domain, string NickName, string Password, string Name, bool Entered)
		{
			RoomNode Node = new RoomNode(this, RoomId, Domain, NickName, Password, Name, Entered);

			lock (this.roomByJid)
			{
				this.roomByJid[RoomId + "@" + Domain] = Node;
			}

			if (this.IsLoaded)
			{
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

			return Node;
		}

		private Task MucClient_RoomInvitationReceived(object Sender, RoomInvitationMessageEventArgs e)
		{
			RoomInvitationReceivedForm Form = new RoomInvitationReceivedForm()
			{
				Owner = MainWindow.currentInstance,
				InviteTo = XmppClient.GetBareJID(e.To),
				InviteFrom = XmppClient.GetBareJID(e.InviteFrom),
				InvitationReason = e.Reason,
				RoomName = e.RoomId,
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
					this.ShowInvitationForm(Form, e, e2);
				});

				return Task.CompletedTask;
			}, null);

			return Task.CompletedTask;
		}

		private void ShowInvitationForm(RoomInvitationReceivedForm Form, RoomInvitationMessageEventArgs e, RoomInformationEventArgs e2)
		{
			bool? b = Form.ShowDialog();

			if (b.HasValue)
			{
				if (b.Value)
				{
					e.Accept(Form.NickName.Text, (sender2, e3) =>
					{
						if (!e3.Ok)
						{
							MainWindow.UpdateGui(() =>
							{
								MessageBox.Show(MainWindow.currentInstance, 
									string.IsNullOrEmpty(e3.ErrorText) ? "Unable to process invitation." : e3.ErrorText,
									"Error", MessageBoxButton.OK, MessageBoxImage.Error);

								RoomInvitationReceivedForm Form2 = new RoomInvitationReceivedForm()
								{
									Owner = MainWindow.currentInstance,
									InviteTo = XmppClient.GetBareJID(e.To),
									InviteFrom = XmppClient.GetBareJID(e.InviteFrom),
									InvitationReason = e.Reason,
									RoomName = Form.RoomName,
									RoomJid = e.RoomId + "@" + e.Domain,
									MembersOnly = e2.MembersOnly,
									Moderated = e2.Moderated,
									NonAnonymous = e2.NonAnonymous,
									Open = e2.Open,
									PasswordProtected = e2.PasswordProtected,
									Persistent = e2.Persistent,
									Public = e2.Public,
									SemiAnonymous = e2.SemiAnonymous,
									Temporary = e2.Temporary,
									Unmoderated = e2.Unmoderated,
									Unsecured = e2.Unsecured
								};

								this.ShowInvitationForm(Form2, e, e2);
							});
						}

						return Task.CompletedTask;
					}, null);
				}
				else
					e.Decline(Form.Reason.Text);
			}
		}

		private Task MucClient_RoomDeclinedInvitationReceived(object Sender, RoomDeclinedMessageEventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Your invitation sent to ");
			sb.Append(e.DeclinedFrom);
			sb.Append(" to join the room ");
			sb.Append(e.RoomId);
			sb.Append("@");
			sb.Append(e.Domain);
			sb.Append(" was declined.");

			if (!string.IsNullOrEmpty(e.Reason))
			{
				sb.Append(" Reason: ");
				sb.Append(e.Reason);
			}

			MainWindow.MessageBox(sb.ToString(), "Invitation declined", MessageBoxButton.OK, MessageBoxImage.Exclamation);

			return Task.CompletedTask;
		}

		private async Task<RoomNode> GetRoomNode(string RoomId, string Domain)
		{
			RoomNode Result;
			string Jid = RoomId + "@" + Domain;

			lock (this.roomByJid)
			{
				if (this.roomByJid.TryGetValue(Jid, out Result))
					return Result;
			}

			string Prefix = this.mucClient.Client.BareJID + "." + Jid;
			string NickName = await RuntimeSettings.GetAsync(Prefix + ".Nick", string.Empty);
			string Password = await RuntimeSettings.GetAsync(Prefix + ".Pwd", string.Empty);

			Result = this.AddRoomNode(RoomId, Domain, NickName, Password, string.Empty, false);

			return Result;
		}

		private async Task MucClient_RoomOccupantMessage(object Sender, RoomOccupantMessageEventArgs e)
		{
			RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);
			string NickName = XmppClient.GetResource(e.From);
			ChatItemType Type = ChatItemType.Received;

			if (!string.IsNullOrEmpty(NickName) && RoomNode.NickName == NickName)
			{
				bool HasDelay = false;

				foreach (XmlNode N in e.Message.ChildNodes)
				{
					if (N is XmlElement E && E.LocalName == "delay")
					{
						HasDelay = true;
						break;
					}
				}

				if (!HasDelay)
					return;

				Type = ChatItemType.Transmitted;
			}

			RoomNode.EnterIfNotAlready(true);

			MainWindow.UpdateGui(() =>
			{
				MainWindow.ParseChatMessage(e, out string Message, out bool IsMarkdown, out DateTime Timestamp);
				MainWindow.currentInstance.MucGroupChatMessage(e.From, XmppClient.GetBareJID(e.To), Message, IsMarkdown, Timestamp, Type, RoomNode, RoomNode.Header);
			});
		}

		private async Task MucClient_RoomSubject(object Sender, RoomOccupantMessageEventArgs e)
		{
			RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);
			if (!RoomNode.Entered)
				return;

			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.MucChatSubject(e.From, XmppClient.GetBareJID(e.To), RoomNode, e.Subject);
			});
		}

		private Task MucClient_RegistrationRequest(object Sender, MessageFormEventArgs e)
		{
			MainWindow.UpdateGui(() =>
			{
				ParameterDialog Dialog = new ParameterDialog(e.Form);
				Dialog.ShowDialog();
			});

			return Task.CompletedTask;
		}

		private Task MucClient_OccupantRequest(object Sender, MessageFormEventArgs e)
		{
			MainWindow.UpdateGui(() =>
			{
				ParameterDialog Dialog = new ParameterDialog(e.Form);
				Dialog.ShowDialog();
			});

			return Task.CompletedTask;
		}

		internal async Task MucClient_OccupantPresence(object Sender, UserPresenceEventArgs e)
		{
			RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);
			OccupantNode OccupantNode = RoomNode.GetOccupantNode(e.NickName, e.Affiliation, e.Role, e.FullJid);
			ChatView View = null;

			if (!OccupantNode.Availability.HasValue || e.Availability != OccupantNode.Availability.Value)
			{
				OccupantNode.Availability = e.Availability;
				OccupantNode.OnUpdated();

				View = MainWindow.currentInstance.FindRoomView(e.From, XmppClient.GetBareJID(e.To));

				if (!(View is null))
				{
					switch (OccupantNode.Availability)
					{
						case Availability.Online:
							View.Event("Online.", e.NickName);
							break;

						case Availability.Offline:
							View.Event("Offline.", e.NickName);
							break;

						case Availability.Away:
							View.Event("Away.", e.NickName);
							break;

						case Availability.Chat:
							View.Event("Ready to chat.", e.NickName);
							break;

						case Availability.DoNotDisturb:
							View.Event("Busy.", e.NickName);
							break;

						case Availability.ExtendedAway:
							View.Event("Away (extended).", e.NickName);
							break;
					}
				}
			}

			await this.MucClient_RoomPresence(Sender, e, View);
		}

		private Task MucClient_RoomPresence(object Sender, UserPresenceEventArgs e)
		{
			return this.MucClient_RoomPresence(Sender, e, null);
		}

		private async Task MucClient_RoomPresence(object _, UserPresenceEventArgs e, ChatView View)
		{
			if ((e.MucStatus?.Length ?? 0) > 0 || e.RoomDestroyed)
			{
				RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);

				if (View is null)
					View = MainWindow.currentInstance.FindRoomView(e.From, XmppClient.GetBareJID(e.To));

				if (!(View is null))
				{
					foreach (MucStatus Status in e.MucStatus ?? new MucStatus[0])
					{
						switch (Status)
						{
							case MucStatus.AffiliationChanged:
								View.Event("New affiliation: " + e.Affiliation.ToString(), e.NickName);
								break;

							case MucStatus.LoggingEnabled:
								View.Event("This room logs messages.", e.NickName);
								break;

							case MucStatus.LoggingDisabled:
								View.Event("This room does not log messages.", e.NickName);
								break;

							case MucStatus.NickModified:
								View.Event("Nick-name changed.", e.NickName);
								break;

							case MucStatus.RoomNonAnonymous:
								View.Event("This room does not anonymous.", e.NickName);
								break;

							case MucStatus.RoomSemiAnonymous:
								View.Event("This room is semi-anonymous.", e.NickName);
								break;

							case MucStatus.RoomAnonymous:
								View.Event("This room does anonymous.", e.NickName);
								break;

							case MucStatus.FullJidVisisble:
								View.Event("All participants in this room have access to the full JID of each other.", e.NickName);
								break;

							case MucStatus.ShowsUnavailableMembers:
								View.Event("This room displays unavailable members.", e.NickName);
								break;

							case MucStatus.DoesNotShowUnavailableMembers:
								View.Event("This room hieds unavailable members.", e.NickName);
								break;

							case MucStatus.NonPrivacyRelatedConfigurationChange:
								View.Event("A configuration that does not affect privacy changed.", e.NickName);
								break;

							case MucStatus.OwnPresence:
								break;

							case MucStatus.Created:
								View.Event("Room created.", e.NickName);
								break;

							case MucStatus.Banned:
								View.Event("Banned from the room.", e.NickName);
								break;

							case MucStatus.NewRoomNickName:
								View.Event("New room nick-name.", e.NickName);
								break;

							case MucStatus.Kicked:
								View.Event("Temporarily kicked from the room.", e.NickName);
								break;

							case MucStatus.RemovedDueToAffiliationChange:
								View.Event("Removed from the room due to an affiliation change.", e.NickName);
								break;

							case MucStatus.RemovedDueToNonMembership:
								View.Event("Removed from the room, since no longer member.", e.NickName);
								break;

							case MucStatus.RemovedDueToSystemShutdown:
								View.Event("Removed from the room due to system shutdown.", e.NickName);
								break;

							case MucStatus.RemovedDueToFailure:
								View.Event("Removed from the room due to technical problems.", e.NickName);
								break;
						}
					}
				}

				if (e.RoomDestroyed)
				{
					View?.Event("Room has been destroyed on the host.", e.NickName);

					RoomNode.Parent.RemoveChild(RoomNode);
					RoomNode.Parent.OnUpdated();
				}
			}
		}

		private async Task MucClient_RoomDestroyed(object Sender, UserPresenceEventArgs e)
		{
			await this.MucClient_RoomPresence(Sender, e);
		}

		private async Task MucClient_PrivateMessageReceived(object Sender, RoomOccupantMessageEventArgs e)
		{
			RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);
			OccupantNode OccupantNode = RoomNode.GetOccupantNode(e.NickName, null, null, null);

			MainWindow.ParseChatMessage(e, out string Message, out bool IsMarkdown, out DateTime Timestamp);

			MainWindow.currentInstance.ChatMessage(e.From, XmppClient.GetBareJID(e.To), Message, IsMarkdown, Timestamp);
		}

		private async Task MucClient_RoomMessage(object Sender, RoomMessageEventArgs e)
		{
			RoomNode RoomNode = await this.GetRoomNode(e.RoomId, e.Domain);

			// TODO
		}

	}
}
