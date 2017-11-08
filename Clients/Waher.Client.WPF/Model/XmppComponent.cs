using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;

namespace Waher.Client.WPF.Model
{
	public class XmppComponent : TreeNode
	{
		private string jid;
		private string name;
		private string node;

		public XmppComponent(TreeNode Parent, string JID, string Name, string Node)
			: base(Parent)
		{
			this.jid = JID;
			this.name = Name;
			this.node = Node;
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
	}
}
