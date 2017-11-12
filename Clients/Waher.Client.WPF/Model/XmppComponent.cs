using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Waher.Networking.XMPP;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model
{
	public class XmppComponent : TreeNode
	{
		private Dictionary<string, bool> features;
		private string jid;
		private string name;
		private string node;
		private bool canSearch;

		public XmppComponent(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent)
		{
			this.jid = JID;
			this.name = Name;
			this.node = Node;
			this.features = Features;
			this.canSearch = this.features.ContainsKey(XmppClient.NamespaceSearch);
		}

		public override string Key => this.jid;
		public override ImageSource ImageResource => XmppAccountNode.component;
		public override string TypeName => "XMPP Server component";
		public override bool CanAddChildren => false;
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
			this.Account?.Client?.SendSearchFormRequest(this.jid, (sender, e) =>
			{
				if (e.Ok)
				{
					MainWindow.currentInstance.Dispatcher.Invoke(() =>
					{
						ParameterDialog Dialog = new ParameterDialog(e.SearchForm);
						Dialog.ShowDialog();
					});
				}
				else
				{
					MainWindow.currentInstance.Dispatcher.Invoke(() => MessageBox.Show(MainWindow.currentInstance,
						string.IsNullOrEmpty(e.ErrorText) ? "Unable to get search form." : e.ErrorText, "Error",
						MessageBoxButton.OK, MessageBoxImage.Error));
				}
			}, (sender, e) =>
			{
				if (e.Ok)
				{
				}
				else
				{
					MainWindow.currentInstance.Dispatcher.Invoke(() => MessageBox.Show(MainWindow.currentInstance,
						string.IsNullOrEmpty(e.ErrorText) ? "Unable to perform search." : e.ErrorText, "Error",
						MessageBoxButton.OK, MessageBoxImage.Error));
				}
			}, null);
		}


	}
}
