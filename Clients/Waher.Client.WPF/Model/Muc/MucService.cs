using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model.Muc
{
	public class MucService : XmppComponent
	{
		private readonly MultiUserChatClient mucClient;

		public MucService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features,
			MultiUserChatClient MucClient)
			: base(Parent, JID, Name, Node, Features)
		{
			this.mucClient = MucClient;

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

		}

		public MultiUserChatClient MucClient
		{
			get { return this.mucClient; }
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

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
				this.Account.Client.SendServiceItemsDiscoveryRequest(this.mucClient.ComponentAddress, (sender, e) =>
				{
					this.loadingChildren = false;
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

						this.NodesRemoved(this.children.Values, this);

						foreach (Item Item in e.Items)
							Children[Item.JID] = new RoomNode(this, Item.JID, Item.Name);

						this.children = new SortedDictionary<string, TreeNode>(Children);
						this.OnUpdated();
						this.NodesAdded(Children.Values, this);
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get rooms." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}

			base.LoadChildren();
		}

	}
}
