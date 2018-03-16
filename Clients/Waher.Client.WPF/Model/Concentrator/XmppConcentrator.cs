using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Windows;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;

namespace Waher.Client.WPF.Model.Concentrator
{
	/// <summary>
	/// Represents an XMPP concentrator.
	/// </summary>
	public class XmppConcentrator : XmppContact
	{
		private Dictionary<string, bool> capabilities = null;
		private bool suportsEvents;

		public XmppConcentrator(TreeNode Parent, XmppClient Client, string BareJid, bool SupportsEventSubscripton)
			: base(Parent, Client, BareJid)
		{
			this.suportsEvents = SupportsEventSubscripton;
			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

			this.CheckCapabilities();
		}

		/// <summary>
		/// If event subscription is supported for readable nodes.
		/// </summary>
		public bool SupportsEvents => this.suportsEvents;

		private void CheckCapabilities()
		{
			if (this.capabilities == null)
			{
				string FullJid = this.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					this.XmppAccountNode.ConcentratorClient.GetCapabilities(FullJid, (sender, e) =>
					{
						if (e.Ok)
						{
							Dictionary<string, bool> Capabilities = new Dictionary<string, bool>();

							foreach (string s in e.Capabilities)
								Capabilities[s] = true;

							this.capabilities = Capabilities;
						}
					}, null);
				}
			}
		}

		public override string TypeName
		{
			get { return "Concentrator"; }
		}

		public string FullJid
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
				if (AccountNode == null || !AccountNode.IsOnline)
					return null;

				RosterItem Item = AccountNode.Client[this.BareJID];
				PresenceEventArgs e = Item?.LastPresence;

				if (e == null || e.Availability == Availability.Offline)
					return null;
				else
					return e.From;
			}
		}

		private bool loadingChildren = false;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				string FullJid = this.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					Mouse.OverrideCursor = Cursors.Wait;

					this.loadingChildren = true;
					this.XmppAccountNode.ConcentratorClient.GetRootDataSources(FullJid, (sender, e) =>
					{
						this.loadingChildren = false;
						MainWindow.MouseDefault();

						if (e.Ok)
						{
							SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

							foreach (DataSourceReference Ref in e.DataSources)
								Children[Ref.SourceID] = new DataSource(this, Ref.SourceID, Ref.SourceID, Ref.HasChildren);

							this.children = Children;

							this.OnUpdated();
							this.NodesAdded(Children.Values, this);
						}
					}, null);
				}
			}

			base.LoadChildren();
		}

		public void NodesAdded(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			if (XmppAccountNode == null)
				return;

			Controls.ConnectionView View = XmppAccountNode.View;
			if (View == null)
				return;

			foreach (TreeNode Node in Nodes)
				View.NodeAdded(Parent, Node);
		}

		public void NodesRemoved(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			XmppAccountNode XmppAccountNode = this.XmppAccountNode;
			if (XmppAccountNode == null)
				return;

			Controls.ConnectionView View = XmppAccountNode.View;
			if (View == null)
				return;

			LinkedList<KeyValuePair<TreeNode, TreeNode>> ToRemove = new LinkedList<KeyValuePair<TreeNode, TreeNode>>();

			foreach (TreeNode Node in Nodes)
				ToRemove.AddLast(new KeyValuePair<TreeNode, TreeNode>(Parent, Node));

			while (ToRemove.First != null)
			{
				KeyValuePair<TreeNode, TreeNode> P = ToRemove.First.Value;
				ToRemove.RemoveFirst();

				Parent = P.Key;
				TreeNode Node = P.Value;

				if (Node.HasChildren.HasValue && Node.HasChildren.Value)
				{
					foreach (TreeNode Child in Node.Children)
						ToRemove.AddLast(new KeyValuePair<TreeNode, TreeNode>(Node, Child));
				}

				View.NodeRemoved(Parent, Node);
			}
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.children == null || this.children.Count != 1 || !this.children.ContainsKey(string.Empty))
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

	}
}
