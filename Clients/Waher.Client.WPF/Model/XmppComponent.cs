using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Waher.Networking.XMPP;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Controls;

namespace Waher.Client.WPF.Model
{
	public class XmppComponent : TreeNode
	{
		private readonly Dictionary<string, bool> features;
		private readonly string jid;
		private readonly string name;
		private readonly string node;
		private readonly bool canSearch;

		public XmppComponent(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent)
		{
			this.jid = JID;
			this.name = Name;
			this.node = Node;
			this.features = Features;
			this.canSearch = this.features.ContainsKey(XmppClient.NamespaceSearch);

			this.Account?.RegisterComponent(this);
		}

		public override void Dispose()
		{
			this.Account?.UnregisterComponent(this);
			base.Dispose();
		}

		public string JID => this.jid;
		public string Name => this.name;
		public string Node => this.node;

		public override string Key => this.jid;
		public override ImageSource ImageResource => XmppAccountNode.component;
		public override string TypeName => "XMPP Server component";
		public override bool CanAddChildren => false;
		public override bool CanEdit => false;
		public override bool CanDelete => false;
		public override bool CanRecycle => false;

		public override string Header
		{
			get
			{
				if (string.IsNullOrEmpty(this.name))
					return this.jid;
				else
					return this.name;
			}
		}

		public override string ToolTip
		{
			get
			{
				if (string.IsNullOrEmpty(this.node))
					return "XMPP Server component";
				else
					return "XMPP Server component (" + this.node + ")";
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		public XmppAccountNode Account
		{
			get { return this.Parent as XmppAccountNode; }
		}

		public override bool CanSearch => this.canSearch;

		public override void Search()
		{
			this.Account?.Client?.SendSearchFormRequest(null, this.jid, (sender, e) =>
			{
				if (e.Ok)
				{
					MainWindow.UpdateGui(() =>
					{
						ParameterDialog Dialog = new ParameterDialog(e.SearchForm);
						Dialog.ShowDialog();
					});
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get search form." : e.ErrorText);
			}, (sender, e) =>
			{
				if (e.Ok)
				{
					MainWindow.UpdateGui(() =>
					{
						TabItem TabItem = MainWindow.NewTab("Search Result");
						MainWindow.currentInstance.Tabs.Items.Add(TabItem);

						SearchResultView View = new SearchResultView(e.Headers, e.Records);
						TabItem.Content = View;

						MainWindow.currentInstance.Tabs.SelectedItem = TabItem;
					});
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to perform search." : e.ErrorText);
			}, null);
		}

		public void NodesAdded(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			XmppAccountNode XmppAccountNode = this.Account;
			if (XmppAccountNode is null)
				return;

			ConnectionView View = XmppAccountNode.View;
			if (View is null)
				return;

			foreach (TreeNode Node in Nodes)
				View.NodeAdded(Parent, Node);
		}

		public void NodesRemoved(IEnumerable<TreeNode> Nodes, TreeNode Parent)
		{
			XmppAccountNode XmppAccountNode = this.Account;
			if (XmppAccountNode is null)
				return;

			Controls.ConnectionView View = XmppAccountNode.View;
			if (View is null)
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

				MainWindow.UpdateGui(() =>
				{
					View.NodeRemoved(Parent, Node);
				});
			}
		}


	}
}
