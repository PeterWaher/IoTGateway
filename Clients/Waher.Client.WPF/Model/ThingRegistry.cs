using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Waher.Client.WPF.Model
{
	public class ThingRegistry : XmppComponent
	{
		private bool supportsProvisioning;

		public ThingRegistry(TreeNode Parent, string JID, string Name, string Node, bool SupportsProvisioning)
			: base(Parent, JID, Name, Node)
		{
			this.supportsProvisioning = SupportsProvisioning;
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				if (this.supportsProvisioning)
					return "Thing Registry & Provisioning Server";
				else
					return "Thing Registry";
			}
		}

	}
}
