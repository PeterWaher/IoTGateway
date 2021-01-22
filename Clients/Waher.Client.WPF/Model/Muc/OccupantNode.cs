using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Waher.Client.WPF.Dialogs.Muc;
using Waher.Content.Markdown;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.MUC;
using Waher.Things.DisplayableParameters;

namespace Waher.Client.WPF.Model.Muc
{
	/// <summary>
	/// Represents an occupant in a room hosted by a Multi-User Chat service.
	/// </summary>
	public class OccupantNode : TreeNode
	{
		private readonly string roomId;
		private readonly string domain;
		private readonly string nickName;
		private string jid;
		private Affiliation? affiliation;
		private Role? role;
		private Availability? availability;

		public OccupantNode(TreeNode Parent, string RoomId, string Domain, string NickName, Affiliation? Affiliation, Role? Role, string Jid)
			: base(Parent)
		{
			this.roomId = RoomId;
			this.domain = Domain;
			this.nickName = NickName;
			this.jid = Jid;
			this.affiliation = Affiliation;
			this.role = Role;
			this.availability = null;

			this.SetParameters();
		}

		public string RoomId => this.roomId;
		public string Domain => this.domain;
		public string NickName => this.nickName;

		public string Jid
		{
			get => this.jid;
			set => this.jid = value;
		}

		public Affiliation? Affiliation
		{
			get => this.affiliation;
			set => this.affiliation = value;
		}

		public Role? Role
		{
			get => this.role;
			set => this.role = value;
		}

		public Availability? Availability
		{
			get => this.availability;
			set => this.availability = value;
		}

		public override void OnUpdated()
		{
			this.SetParameters();
			base.OnUpdated();
		}

		private void SetParameters()
		{
			List<Parameter> Parameters = new List<Parameter>()
			{
				new StringParameter("RoomID", "Room ID", this.roomId),
				new StringParameter("Domain", "Domain", this.domain),
				new StringParameter("NickName", "Nick-Name", this.nickName)
			};

			if (!string.IsNullOrEmpty(this.jid))
				Parameters.Add(new StringParameter("JID", "JID", this.jid));

			if (this.affiliation.HasValue)
				Parameters.Add(new StringParameter("Affiliation", "Affiliation", this.Affiliation.ToString()));

			if (this.role.HasValue)
				Parameters.Add(new StringParameter("Role", "Role", this.Role.ToString()));

			if (this.availability.HasValue)
				Parameters.Add(new StringParameter("Availability", "Availability", this.Availability.ToString()));

			this.parameters = new DisplayableParameters(Parameters.ToArray());
		}

		public override string ToolTip => this.Jid;
		public override bool CanRecycle => false;

		public override string TypeName
		{
			get
			{
				return "Occupant";
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				if (!this.availability.HasValue)
					return XmppAccountNode.offline;

				switch (this.availability.Value)
				{
					case Networking.XMPP.Availability.Away:
						return XmppAccountNode.away;

					case Networking.XMPP.Availability.Chat:
						return XmppAccountNode.chat;

					case Networking.XMPP.Availability.DoNotDisturb:
						return XmppAccountNode.busy;

					case Networking.XMPP.Availability.ExtendedAway:
						return XmppAccountNode.away;


					case Networking.XMPP.Availability.Online:
						return XmppAccountNode.online;

					case Networking.XMPP.Availability.Offline:
					default:
						return XmppAccountNode.offline;
				}
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		public MucService Service
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is MucService MucService)
						return MucService;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		public MultiUserChatClient MucClient
		{
			get
			{
				return this.Service?.MucClient;
			}
		}

		public override string Key => this.nickName;
		public override string Header => this.nickName;
		public override bool CanAddChildren => false;
		public override bool CanDelete => true;
		public override bool CanEdit => true;
		public override bool CanChat => true;

		public override void Delete(TreeNode Parent, EventHandler OnDeleted)
		{
			this.MucClient?.Kick(this.RoomId, this.domain, this.nickName, null, null);
			base.Delete(Parent, OnDeleted);
		}

		public override void Edit()
		{
			OccupantPrivilegesForm Form = new OccupantPrivilegesForm();

			Form.Affiliation.SelectedIndex = (int)this.affiliation;
			Form.Role.SelectedIndex = (int)this.role;

			if (string.IsNullOrEmpty(this.jid))
				Form.Affiliation.IsEnabled = false;

			bool? b = Form.ShowDialog();
			if (b.HasValue && b.Value)
			{
				Affiliation NewAffiliation = (Affiliation)Form.Affiliation.SelectedIndex;
				Role NewRole = (Role)Form.Role.SelectedIndex;
				string Reason = Form.Reason.Text;

				Task.Run(() => this.Config(NewAffiliation, NewRole, Reason));
			}
		}

		private async Task Config(Affiliation Affiliation, Role Role, string Reason)
		{
			try
			{
				if (this.affiliation != Affiliation)
				{
					await this.MucClient.ConfigureOccupantAsync(this.roomId, this.domain, XmppClient.GetBareJID(this.jid), Affiliation, Reason);
					this.affiliation = Affiliation;
					this.OnUpdated();
				}

				if (this.role != Role)
				{
					await this.MucClient.ConfigureOccupantAsync(this.roomId, this.domain, this.nickName, Role, Reason);
					this.role = Role;
					this.OnUpdated();
				}
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		public override void SendChatMessage(string Message, string ThreadId, MarkdownDocument Markdown)
		{
			if (Markdown is null)
				this.MucClient.SendPrivateMessage(this.roomId, this.domain, this.nickName, Message, string.Empty, ThreadId);
			else
			{
				this.MucClient.SendCustomPrivateMessage(this.roomId, this.domain, this.nickName,
					XmppContact.MultiFormatMessage(Message, Markdown), string.Empty, ThreadId);
			}
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			MenuItem Item;
			MenuItem Affiliation;
			MenuItem Role;

			this.GroupSeparator(ref CurrentGroup, "MUC", Menu);

			Menu.Items.Add(Affiliation = new MenuItem()
			{
				Header = "_Affiliation",
				IsEnabled = true,
			});

			Affiliation.Items.Add(Item = new MenuItem()
			{
				Header = "Owner",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.affiliation == Networking.XMPP.MUC.Affiliation.Owner
			});

			Item.Click += this.SetAffiliationOwner_Click;

			Affiliation.Items.Add(Item = new MenuItem()
			{
				Header = "Administrator",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.affiliation == Networking.XMPP.MUC.Affiliation.Admin
			});

			Item.Click += this.SetAffiliationAdministrator_Click;

			Affiliation.Items.Add(Item = new MenuItem()
			{
				Header = "Member",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.affiliation == Networking.XMPP.MUC.Affiliation.Member
			});

			Item.Click += this.SetAffiliationMember_Click;

			Affiliation.Items.Add(Item = new MenuItem()
			{
				Header = "None",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.affiliation == Networking.XMPP.MUC.Affiliation.None
			});

			Item.Click += this.SetAffiliationNone_Click;

			Affiliation.Items.Add(Item = new MenuItem()
			{
				Header = "Outcast",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.affiliation == Networking.XMPP.MUC.Affiliation.Outcast
			});

			Item.Click += this.SetAffiliationOutcast_Click;

			Menu.Items.Add(Role = new MenuItem()
			{
				Header = "_Role",
				IsEnabled = true,
			});

			Role.Items.Add(Item = new MenuItem()
			{
				Header = "Moderator",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.role == Networking.XMPP.MUC.Role.Moderator
			});

			Item.Click += this.SetRoleModerator_Click;

			Role.Items.Add(Item = new MenuItem()
			{
				Header = "Participant",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.role == Networking.XMPP.MUC.Role.Participant
			});

			Item.Click += this.SetRoleParticipant_Click;

			Role.Items.Add(Item = new MenuItem()
			{
				Header = "Visitor",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.role == Networking.XMPP.MUC.Role.Visitor
			});

			Item.Click += this.SetRoleVisitor_Click;

			Role.Items.Add(Item = new MenuItem()
			{
				Header = "None",
				IsEnabled = true,
				IsCheckable = true,
				IsChecked = this.role == Networking.XMPP.MUC.Role.None
			});

			Item.Click += this.SetRoleNone_Click;

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "_Ban...",
				IsEnabled = true,
			});

			Item.Click += this.Ban_Click;
		}

		private void Ban_Click(object sender, RoutedEventArgs e)
		{
			BanOccupantForm Form = new BanOccupantForm(this.nickName);
			bool? b = Form.ShowDialog();

			if (b.HasValue && b.Value)
			{
				this.MucClient.Ban(this.roomId, this.domain, this.jid, Form.Reason.Text, (sender2, e2) =>
				{
					if (e2.Ok)
						this.Delete(this.Parent, null);
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to ban the occupant." : e2.ErrorText);

					return Task.CompletedTask;
				}, null);
			}
		}

		private void SetAffiliationOwner_Click(object sender, RoutedEventArgs e)
		{
			this.SetAffiliation(Networking.XMPP.MUC.Affiliation.Owner);
		}

		private void SetAffiliationAdministrator_Click(object sender, RoutedEventArgs e)
		{
			this.SetAffiliation(Networking.XMPP.MUC.Affiliation.Admin);
		}

		private void SetAffiliationMember_Click(object sender, RoutedEventArgs e)
		{
			this.SetAffiliation(Networking.XMPP.MUC.Affiliation.Member);
		}

		private void SetAffiliationNone_Click(object sender, RoutedEventArgs e)
		{
			this.SetAffiliation(Networking.XMPP.MUC.Affiliation.None);
		}

		private void SetAffiliationOutcast_Click(object sender, RoutedEventArgs e)
		{
			this.SetAffiliation(Networking.XMPP.MUC.Affiliation.Outcast);
		}

		private void SetAffiliation(Affiliation Affiliation)
		{
			this.MucClient.ConfigureOccupant(this.roomId, this.domain, XmppClient.GetBareJID(this.jid), Affiliation, string.Empty, (sender, e) =>
			{
				if (e.Ok)
				{
					this.affiliation = Affiliation;
					this.OnUpdated();
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to change affiliation." : e.ErrorText);

				return Task.CompletedTask;
			}, null);
		}

		private void SetRoleModerator_Click(object sender, RoutedEventArgs e)
		{
			this.SetRole(Networking.XMPP.MUC.Role.Moderator);
		}

		private void SetRoleParticipant_Click(object sender, RoutedEventArgs e)
		{
			this.SetRole(Networking.XMPP.MUC.Role.Participant);
		}

		private void SetRoleVisitor_Click(object sender, RoutedEventArgs e)
		{
			this.SetRole(Networking.XMPP.MUC.Role.Visitor);
		}

		private void SetRoleNone_Click(object sender, RoutedEventArgs e)
		{
			this.SetRole(Networking.XMPP.MUC.Role.None);
		}

		private void SetRole(Role Role)
		{
			this.MucClient.ConfigureOccupant(this.roomId, this.domain, this.nickName, Role, string.Empty, (sender, e) =>
			{
				if (e.Ok)
				{
					this.role = Role;
					this.OnUpdated();
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to change role." : e.ErrorText);

				return Task.CompletedTask;
			}, null);
		}

	}
}
