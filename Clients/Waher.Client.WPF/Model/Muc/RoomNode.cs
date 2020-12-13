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
	/// Represents a room hosted by a Multi-User Chat service.
	/// </summary>
	public class RoomNode : TreeNode
	{
		private readonly string jid;
		private readonly string roomId;
		private readonly string domain;
		private readonly string name;

		public RoomNode(TreeNode Parent, string Jid, string Name)
			: base(Parent)
		{
			this.jid = Jid;
			this.name = Name;

			int i = Jid.IndexOf('@');
			if (i < 0)
			{
				this.roomId = string.Empty;
				this.domain = Jid;
			}
			else
			{
				this.roomId = Jid.Substring(0, i);
				this.domain = Jid.Substring(i + 1);
			}

			this.SetParameters();
		}

		public override string Key => this.Jid;
		public override string Header => this.Jid;
		public string RoomId => this.roomId;
		public string Domain => this.domain;
		public string Jid => this.jid;

		private void SetParameters()
		{
			List<Parameter> Parameters = new List<Parameter>();

			if (!string.IsNullOrEmpty(this.jid))
				Parameters.Add(new StringParameter("JID", "JID", this.jid));

			if (!string.IsNullOrEmpty(this.name))
				Parameters.Add(new StringParameter("Name", "Name", this.name));

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

			this.parameters = new DisplayableParameters(Parameters.ToArray());
		}

		public override string ToolTip => this.name;
		public override bool CanRecycle => false;

		public override string TypeName
		{
			get
			{
				return "Multi-User Chat Room";
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				if (this.IsExpanded)
					return XmppAccountNode.folderOpen;
				else
					return XmppAccountNode.folderClosed;
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

		private bool loadingChildren = false;

		public MultiUserChatClient MucClient
		{
			get
			{
				return this.Service?.MucClient;
			}
		}

		public override bool CanAddChildren => true;
		public override bool CanDelete => true;
		public override bool CanEdit => true;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && !this.IsLoaded)
			{
				Mouse.OverrideCursor = Cursors.Wait;
				this.loadingChildren = true;

				this.MucClient?.GetOccupants(this.roomId, this.domain, null, null, (sender, e) =>
				{
					this.loadingChildren = false;
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

						this.Service.NodesRemoved(this.children.Values, this);

						foreach (MucOccupant Occupant in e.Occupants)
							Children[Occupant.Jid] = new OccupantNode(this, Occupant);

						this.children = new SortedDictionary<string, TreeNode>(Children);
						this.OnUpdated();
						this.Service.NodesAdded(Children.Values, this);
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get occupants." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}

			base.LoadChildren();
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.IsLoaded)
			{
				if (this.children != null)
					this.Service?.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}

	}
}
