using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a node in a concentrator.
	/// </summary>
	public class Node : TreeNode
	{
		private NodeInformation nodeInfo;

		public Node(TreeNode Parent, NodeInformation NodeInfo)
			: base(Parent)
		{
			this.nodeInfo = NodeInfo;

			if (nodeInfo.HasChildren)
			{
				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};
			}
		}

		public override string Key => this.nodeInfo.NodeId;
		public override string Header => this.nodeInfo.DisplayName;
		public override string ToolTip => "Node";
		public override string TypeName => this.nodeInfo.NodeType;
		public override bool CanAddChildren => false;
		public override bool CanRecycle => false;

		public override ImageSource ImageResource
		{
			get
			{
				if (this.nodeInfo.HasChildren)
				{
					if (this.IsExpanded)
						return XmppAccountNode.folderOpen;
					else
						return XmppAccountNode.folderClosed;
				}
				else
					return XmppAccountNode.box;
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

				while (Loop!=null)
				{
					if (Loop is XmppConcentrator Concentrator)
						return Concentrator;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		protected override void OnExpanded()
		{
			if (this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				string FullJid = this.Concentrator?.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					Mouse.OverrideCursor = Cursors.Wait;

					if (this.nodeInfo.HasChildren)
					{
						Concentrator.XmppAccountNode.ConcentratorClient.GetChildNodes(FullJid, this.nodeInfo, true, true,
							"en", string.Empty, string.Empty, string.Empty, (sender, e) =>
						{
							Mouse.OverrideCursor = null;

							if (e.Ok)
							{
								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (NodeInformation Ref in e.NodesInformation)
									Children[Ref.NodeId] = new Node(this, Ref);

								this.children = Children;

								this.OnUpdated();
							}
						}, null);
					}
					else
					{
						this.children = null;
						this.OnUpdated();
					}
				}
			}

			base.OnExpanded();
		}

		protected override void OnCollapsed()
		{
			base.OnCollapsed();

			if (this.nodeInfo.HasChildren && (this.children == null || this.children.Count != 1 || !this.children.ContainsKey(string.Empty)))
			{
				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}
	}
}
