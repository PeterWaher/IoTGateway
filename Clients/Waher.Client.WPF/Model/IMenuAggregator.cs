using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Interface for nodes that can aggregate items in menues to descendant nodes.
	/// </summary>
	public interface IMenuAggregator
	{
		/// <summary>
		/// Adds context menu items for a node.
		/// </summary>
		/// <param name="Node">Node to whom context menu items will be added.</param>
		/// <param name="CurrentGroup">Current group.</param>
		/// <param name="Menu">Menu being built.</param>
		void AddContexMenuItems(TreeNode Node, ref string CurrentGroup, ContextMenu Menu);
	}
}
