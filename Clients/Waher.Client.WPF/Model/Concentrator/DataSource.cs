using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Things.SourceEvents;

namespace Waher.Client.WPF.Model.Concentrator
{
	/// <summary>
	/// Represents a data source in a concentrator.
	/// </summary>
	public class DataSource : TreeNode
	{
		private Timer timer = null;
		private string key;
		private string header;
		private bool hasChildSources;
		private bool unsubscribed = false;

		public DataSource(TreeNode Parent, string Key, string Header, bool HasChildSources)
			: base(Parent)
		{
			this.key = Key;
			this.header = Header;
			this.hasChildSources = HasChildSources;

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};
		}

		public override void Dispose()
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			base.Dispose();
		}

		public override string Key => this.key;
		public override string Header => this.header;
		public override string ToolTip => "Data source";
		public override string TypeName => "Data Source";
		public override bool CanAddChildren => false;
		public override bool CanEdit => false;
		public override bool CanDelete => false;
		public override bool CanRecycle => false;

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

		public XmppConcentrator Concentrator
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is XmppConcentrator Concentrator)
						return Concentrator;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		private bool loadingChildren = false;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				string FullJid = this.Concentrator?.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					Mouse.OverrideCursor = Cursors.Wait;

					if (this.hasChildSources)
					{
						this.loadingChildren = true;
						Concentrator.XmppAccountNode.ConcentratorClient.GetChildDataSources(FullJid, this.key, (sender, e) =>
						{
							this.loadingChildren = false;
							MainWindow.MouseDefault();

							if (e.Ok)
							{
								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (DataSourceReference Ref in e.DataSources)
								{
									DataSource DataSource = new DataSource(this, Ref.SourceID, Ref.SourceID, Ref.HasChildren);
									Children[Ref.SourceID] = DataSource;

									DataSource.SubscribeToEvents();
								}

								this.children = Children;

								this.OnUpdated();
								this.Concentrator?.NodesAdded(Children.Values, this);
							}
						}, null);
					}
					else
					{
						this.loadingChildren = true;
						Concentrator.XmppAccountNode.ConcentratorClient.GetRootNodes(FullJid, this.key, true, true,
							"en", string.Empty, string.Empty, string.Empty, (sender, e) =>
						{
							this.loadingChildren = false;
							MainWindow.MouseDefault();

							if (e.Ok)
							{
								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (NodeInformation Ref in e.NodesInformation)
									Children[Ref.NodeId] = new Node(this, Ref);

								this.children = Children;

								this.OnUpdated();
								this.Concentrator?.NodesAdded(Children.Values, this);
							}
						}, null);
					}
				}
			}

			base.LoadChildren();
		}

		public void SubscribeToEvents()
		{
			if (this.unsubscribed)
				return;

			XmppConcentrator Concentrator = this.Concentrator;
			if (Concentrator == null)
				return;

			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			if (XmppAccountNode == null)
				return;

			string FullJid = Concentrator.FullJid;
			ConcentratorClient ConcentratorClient = XmppAccountNode?.ConcentratorClient;
			if (ConcentratorClient == null)
				return;

			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			ConcentratorClient.Subscribe(FullJid, this.key, 600, SourceEventType.All, true, true, "en",
				string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					if (e.Ok)
					{
						this.timer = new Timer((P) =>
						{
							this.SubscribeToEvents();
						}, null, 300000, 300000);
					}
				}, null);
		}

		public void UnsubscribeFromEvents()
		{
			this.unsubscribed = true;

			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			XmppConcentrator Concentrator = this.Concentrator;
			if (Concentrator == null)
				return;

			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			if (XmppAccountNode == null)
				return;

			string FullJid = Concentrator.FullJid;
			ConcentratorClient ConcentratorClient = XmppAccountNode?.ConcentratorClient;
			if (ConcentratorClient == null)
				return;

			XmppAccountNode.ConcentratorClient.Unsubscribe(FullJid, this.key, SourceEventType.All, "en",
				string.Empty, string.Empty, string.Empty, null, null);
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.children == null || this.children.Count != 1 || !this.children.ContainsKey(string.Empty))
			{
				if (this.children != null)
					this.Concentrator?.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}
	}
}
