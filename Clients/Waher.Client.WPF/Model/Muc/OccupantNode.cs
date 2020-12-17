using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml;
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
			// TODO: Edit affiliation & role
			base.Edit();
		}

		public override void SendChatMessage(string Message, MarkdownDocument Markdown)
		{
			// TODO: Send private message
			base.SendChatMessage(Message, Markdown);
		}

	}
}
