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
	/// Represents a data source in a concentrator.
	/// </summary>
	public class Loading : TreeNode
	{
		public Loading(TreeNode Parent)
			: base(Parent)
		{
		}

		public override string Key => string.Empty;
		public override string Header => "Loading...";
		public override string ToolTip => "Items are being loaded.";
		public override string TypeName => string.Empty;
		public override bool CanAddChildren => false;
		public override bool CanRecycle => false;
		public override ImageSource ImageResource => XmppAccountNode.hourglass;

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}
	}
}
