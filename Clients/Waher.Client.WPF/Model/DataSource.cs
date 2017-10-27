using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a data source in a concentrator.
	/// </summary>
	public class DataSource : TreeNode
	{
		private string key;
		private string header;
		private bool hasChildSources;

		public DataSource(TreeNode Parent, string Key, string Header, bool HasChildSources)
			: base(Parent)
		{
			this.key = Key;
			this.header = Header;
			this.hasChildSources = HasChildSources;
		}

		public override string Key => this.key;
		public override string Header => this.header;
		public override string ToolTip => "Data source";
		public override string TypeName => "Data Source";
		public override bool CanAddChildren => false;
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
	}
}
