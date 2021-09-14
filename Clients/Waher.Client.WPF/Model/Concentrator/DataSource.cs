using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		private readonly Dictionary<string, Node> nodes = new Dictionary<string, Node>();
		private Timer timer = null;
		private readonly string key;
		private readonly string header;
		private readonly bool hasChildSources;
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
			this.timer?.Dispose();
			this.timer = null;

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
					return XmppAccountNode.folderYellowOpen;
				else
					return XmppAccountNode.folderYellowClosed;
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
			if (!this.loadingChildren && !this.IsLoaded)
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

							return Task.CompletedTask;
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
								this.NodesAdded(Children.Values, this);
							}

							return Task.CompletedTask;
						}, null);
					}
				}
			}

			base.LoadChildren();
		}

		public void NodesAdded(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			lock (this.nodes)
			{
				foreach (TreeNode Node in Nodes)
				{
					if (Node is Node Node2)
					{
						string Key = Node2.Partition + " " + Node2.NodeId;
						this.nodes[Key] = Node2;
					}
				}
			}

			this.Concentrator?.NodesAdded(Nodes, Parent);
		}

		public void NodesRemoved(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			lock (this.nodes)
			{
				foreach (TreeNode Node in Nodes)
				{
					if (Node is Node Node2)
					{
						string Key = Node2.Partition + " " + Node2.NodeId;
						this.nodes.Remove(Key);
					}
				}
			}

			this.Concentrator?.NodesRemoved(Nodes, Parent);
		}

		public void SubscribeToEvents()
		{
			if (this.unsubscribed)
				return;

			XmppConcentrator Concentrator = this.Concentrator;
			if (Concentrator is null)
				return;

			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			if (XmppAccountNode is null)
				return;

			string FullJid = Concentrator.FullJid;
			ConcentratorClient ConcentratorClient = XmppAccountNode?.ConcentratorClient;
			if (ConcentratorClient is null)
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

					return Task.CompletedTask;
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
			if (Concentrator is null)
				return;

			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			if (XmppAccountNode is null)
				return;

			string FullJid = Concentrator.FullJid;
			ConcentratorClient ConcentratorClient = XmppAccountNode?.ConcentratorClient;
			if (ConcentratorClient is null)
				return;

			XmppAccountNode.ConcentratorClient.Unsubscribe(FullJid, this.key, SourceEventType.All, "en",
				string.Empty, string.Empty, string.Empty, null, null);
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.IsLoaded)
			{
				if (this.children != null)
					this.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}

		internal Task ConcentratorClient_OnEvent(object Sender, SourceEventMessageEventArgs EventMessage)
		{
			SourceEvent Event = EventMessage.Event;
			string Key, ParentKey;
			Node Node;

			switch (Event.EventType)
			{
				case SourceEventType.NodeAdded:
					if (Event is NodeAdded NodeAdded)
					{
						Key = NodeAdded.Partition + " " + NodeAdded.NodeId;
						ParentKey = NodeAdded.ParentPartition + " " + NodeAdded.ParentId;
						Node Parent;

						lock (this.nodes)
						{
							if (this.nodes.ContainsKey(Key))
								return Task.CompletedTask; // Already added

							if (!this.nodes.TryGetValue(ParentKey, out Parent))
								return Task.CompletedTask; // Parent not loaded.

							Node = new Node(Parent, new NodeInformation(NodeAdded.NodeId, NodeAdded.SourceId, NodeAdded.Partition,
								NodeAdded.NodeType, NodeAdded.DisplayName, NodeAdded.State, NodeAdded.LocalId, NodeAdded.LogId,
								NodeAdded.HasChildren, NodeAdded.ChildrenOrdered, NodeAdded.IsReadable, NodeAdded.IsControllable,
								NodeAdded.HasCommands, NodeAdded.Sniffable, NodeAdded.ParentId, NodeAdded.ParentPartition,
								NodeAdded.Updated, NodeAdded.Parameters, null));

							this.nodes[Key] = Node;
						}

						Parent.Add(Node);
					}
					break;

				case SourceEventType.NodeUpdated:
					if (Event is NodeUpdated NodeUpdated)
					{
						Key = NodeUpdated.Partition + " " + NodeUpdated.NodeId;

						lock (this.nodes)
						{
							if (!this.nodes.TryGetValue(Key, out Node))
							{
								if (string.IsNullOrEmpty(NodeUpdated.OldId))
									return Task.CompletedTask; // Parent not loaded.

								string OldKey = NodeUpdated.Partition + " " + NodeUpdated.OldId;
								if (!this.nodes.TryGetValue(OldKey, out Node))
									return Task.CompletedTask; // Parent not loaded.

								this.nodes.Remove(OldKey);
								this.nodes[Key] = Node;
							}
						}

						Node.NodeInformation = new NodeInformation(NodeUpdated.NodeId, NodeUpdated.SourceId, NodeUpdated.Partition,
							Node.NodeInformation.NodeType, Node.NodeInformation.DisplayName, NodeUpdated.State, NodeUpdated.LocalId, NodeUpdated.LogId,
							NodeUpdated.HasChildren, NodeUpdated.ChildrenOrdered, NodeUpdated.IsReadable, NodeUpdated.IsControllable,
							NodeUpdated.HasCommands, Node.NodeInformation.Sniffable, NodeUpdated.ParentId, NodeUpdated.ParentPartition,
							NodeUpdated.Updated, NodeUpdated.Parameters, null);

						Node.OnUpdated();
					}
					break;

				case SourceEventType.NodeRemoved:
					if (Event is NodeRemoved NodeRemoved)
					{
						Key = NodeRemoved.Partition + " " + NodeRemoved.NodeId;

						lock (this.nodes)
						{
							if (!this.nodes.TryGetValue(Key, out Node))
								return Task.CompletedTask; // Parent not loaded.

							this.nodes.Remove(Key);
						}

						Node.Parent.RemoveChild(Node);
						this.NodesRemoved(new TreeNode[] { Node }, Node.Parent);
					}
					break;

				case SourceEventType.NodeStatusChanged:
					if (Event is NodeStatusChanged NodeStatusChanged)
					{
						Key = NodeStatusChanged.Partition + " " + NodeStatusChanged.NodeId;

						lock (this.nodes)
						{
							if (!this.nodes.TryGetValue(Key, out Node))
								return Task.CompletedTask; // Parent not loaded.
						}

						Node.NodeInformation = new NodeInformation(NodeStatusChanged.NodeId, NodeStatusChanged.SourceId, NodeStatusChanged.Partition,
							Node.NodeInformation.NodeType, Node.NodeInformation.DisplayName, NodeStatusChanged.State, NodeStatusChanged.LocalId, NodeStatusChanged.LogId,
							Node.NodeInformation.HasChildren, Node.NodeInformation.ChildrenOrdered, Node.NodeInformation.IsReadable, Node.NodeInformation.IsControllable,
							Node.NodeInformation.HasCommands, Node.NodeInformation.Sniffable, Node.NodeInformation.ParentId, Node.NodeInformation.ParentPartition,
							Node.NodeInformation.LastChanged, Node.NodeInformation.ParameterList, null);

						Node.OnUpdated();
					}
					break;

				case SourceEventType.NodeMovedUp:
					if (Event is NodeMovedUp NodeMovedUp)
					{
						Key = NodeMovedUp.Partition + " " + NodeMovedUp.NodeId;

						lock (this.nodes)
						{
							if (!this.nodes.TryGetValue(Key, out Node))
								return Task.CompletedTask; // Parent not loaded.
						}

						// TODO: Node.Parent?.MoveUp(Node);
					}
					break;

				case SourceEventType.NodeMovedDown:
					if (Event is NodeMovedDown NodeMovedDown)
					{
						Key = NodeMovedDown.Partition + " " + NodeMovedDown.NodeId;

						lock (this.nodes)
						{
							if (!this.nodes.TryGetValue(Key, out Node))
								return Task.CompletedTask; // Parent not loaded.
						}

						// TODO: Node.Parent?.MoveDown(Node);
					}
					break;
			}

			return Task.CompletedTask;
		}

	}
}
