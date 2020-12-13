using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Things.DisplayableParameters;
using Waher.Client.WPF.Dialogs;
using System.Windows.Controls;

namespace Waher.Client.WPF.Model.Muc
{
	/// <summary>
	/// Represents an occupant in a room hosted by a Multi-User Chat service.
	/// </summary>
	public class OccupantNode : TreeNode
	{
		private readonly MucOccupant occupant;

		public OccupantNode(TreeNode Parent, MucOccupant Occupant)
			: base(Parent)
		{
			this.occupant = Occupant;
			this.SetParameters();
		}

		public string RoomId => this.occupant.RoomId;
		public string Domain => this.occupant.Domain;
		public string NickName => this.occupant.NickName;
		public string Jid => this.occupant.Jid;
		public Affiliation Affiliation => this.occupant.Affiliation;
		public Role Role => this.occupant.Role;

		private void SetParameters()
		{
			this.parameters = new DisplayableParameters(new Parameter[]
			{
				new StringParameter("NickName", "Nick-Name", this.NickName),
				new StringParameter("Affiliation", "Affiliation", this.Affiliation.ToString()),
				new StringParameter("Role", "Role", this.Role.ToString())
			});
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
				return XmppAccountNode.offline;
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

		public override string Key => this.Jid;
		public override string Header => this.Jid;
		public override bool CanAddChildren => false;
		public override bool CanDelete => true;
		public override bool CanEdit => true;

	}
}
