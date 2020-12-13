using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Waher.Client.WPF.Dialogs;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Settings;

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

				this.mucClient.EnterRoom(RoomId, Domain, NickName, Password, async (sender, e) =>
				{
					if (e.Ok)
					{
						if (e.HasStatus(MucStatus.Created))
						{
							this.mucClient.GetRoomConfiguration(RoomId, Domain, (sender2, ConfigurationForm) =>
							{
								MainWindow.currentInstance.ShowDataForm(ConfigurationForm);
								return Task.CompletedTask;
							}, async (sender2, e2) =>
							{
								if (e2.Ok)
								{
									await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Nick", NickName);
									await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Pwd", Password);

									this.AddRoomNode(RoomId, Domain, NickName, Password);
								}
								else
									MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to configure room." : e.ErrorText);
							}, null);
						}
						else
						{
							await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Nick", NickName);
							await RuntimeSettings.SetAsync(RoomId + "@" + Domain + ".Pwd", Password);

							this.AddRoomNode(RoomId, Domain, NickName, Password);
						}
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to add room." : e.ErrorText);
				}, null);
			}
		}

		private void AddRoomNode(string RoomId, string Domain, string NickName, string Password)
		{
			if (this.IsLoaded)
			{
				RoomNode Node = new RoomNode(this, RoomId, Domain, NickName, Password, string.Empty, true);

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
					this.OnUpdated();
				});
			}
		}

	}
}
