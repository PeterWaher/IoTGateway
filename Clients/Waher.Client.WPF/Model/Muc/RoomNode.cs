using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Client.WPF.Dialogs;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.MUC;
using Waher.Runtime.Settings;
using Waher.Things.DisplayableParameters;

namespace Waher.Client.WPF.Model.Muc
{
	/// <summary>
	/// Represents a room hosted by a Multi-User Chat service.
	/// </summary>
	public class RoomNode : TreeNode
	{
		private readonly string roomId;
		private readonly string domain;
		private readonly string name;
		private string nickName;
		private string password;
		private bool entered;

		public RoomNode(TreeNode Parent, string RoomId, string Domain, string NickName, string Password, string Name, bool Entered)
			: base(Parent)
		{
			this.roomId = RoomId;
			this.domain = Domain;
			this.name = Name;
			this.nickName = NickName;
			this.password = Password;
			this.entered = Entered;

			this.SetParameters();
		}

		public override string Key => this.Jid;
		public override string Header => string.IsNullOrEmpty(this.name) ? this.Jid : this.name;
		public string RoomId => this.roomId;
		public string Domain => this.domain;
		public string Jid => this.roomId + "@" + this.domain;

		private void SetParameters()
		{
			List<Parameter> Parameters = new List<Parameter>()
			{
				new StringParameter("RoomID", "Room ID", this.roomId),
				new StringParameter("Domain", "Domain", this.domain)
			};

			if (!string.IsNullOrEmpty(this.name))
				Parameters.Add(new StringParameter("Name", "Name", this.name));

			if (!string.IsNullOrEmpty(this.nickName))
				Parameters.Add(new StringParameter("NickName", "Nick-Name", this.nickName));

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

		protected override async void LoadChildren()
		{
			try
			{
				if (!this.loadingChildren && !this.IsLoaded)
				{
					if (!await this.AssertEntered())
						return;

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
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
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

		private async Task<bool> AssertEntered()
		{
			if (!this.entered)
			{
				UserPresenceEventArgs e;
				EnterRoomForm Form = null;

				if (string.IsNullOrEmpty(this.nickName))
					e = null;
				else
					e = await this.MucClient.EnterRoomAsync(this.roomId, this.domain, this.nickName, this.password);

				while (!(e?.Ok ?? false))
				{
					TaskCompletionSource<bool> InputReceived = new TaskCompletionSource<bool>();

					if (Form is null)
					{
						Form = new EnterRoomForm(this.roomId)
						{
							Owner = MainWindow.currentInstance
						};

						Form.NickName.Text = this.nickName;
						Form.Password.Password = this.password;
					}

					MainWindow.UpdateGui(() =>
					{
						bool? Result = Form.ShowDialog();
						InputReceived.TrySetResult(Result.HasValue && Result.Value);
					});

					if (!await InputReceived.Task)
						return false;

					e = await this.MucClient.EnterRoomAsync(this.roomId, this.domain, Form.NickName.Text, Form.Password.Password);
					if (!e.Ok)
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to configure room." : e.ErrorText);
				}

				if (!(Form is null))
				{
					if (this.nickName != Form.NickName.Text)
					{
						this.nickName = Form.NickName.Text;
						await RuntimeSettings.SetAsync(this.roomId + "@" + this.domain + ".Nick", this.nickName);
					}

					if (this.password != Form.Password.Password)
					{
						this.password = Form.Password.Password;
						await RuntimeSettings.SetAsync(this.roomId + "@" + this.domain + ".Pwd", this.password);
					}
				}

				this.entered = true;
			}

			return true;
		}

		public override async void Edit()
		{
			try
			{
				if (!await this.AssertEntered())
					return;

				DataForm ConfigurationForm = await this.MucClient.GetRoomConfigurationAsync(this.roomId, this.domain, (sender, e) =>
				{
					if (e.Ok)
						this.OnUpdated();
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to configure room." : e.ErrorText);

					return Task.CompletedTask;
				}, null);

				MainWindow.currentInstance.ShowDataForm(ConfigurationForm);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		public override void Delete(TreeNode Parent, EventHandler OnDeleted)
		{
			// TODO
			base.Delete(Parent, OnDeleted);
		}

		public override void Add()
		{
			// TODO
		}

	}
}
