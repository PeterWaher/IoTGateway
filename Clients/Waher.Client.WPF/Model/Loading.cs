using System.Windows.Media;
using System.Xml;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a data source in a concentrator.
	/// </summary>
	public class Loading(TreeNode Parent) 
		: TreeNode(Parent)
	{
		public override string Key => string.Empty;
		public override string ToolTip => "Items are being loaded.";
		public override string TypeName => string.Empty;
		public override bool CanAddChildren => false;
		public override bool CanEdit => false;
		public override bool CanDelete => false;
		public override bool CanRecycle => false;
		public override ImageSource ImageResource => XmppAccountNode.hourglass;

		public override string Header
		{
			get
			{
				this.LoadSiblings();
				return "Loading...";
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}
	}
}
