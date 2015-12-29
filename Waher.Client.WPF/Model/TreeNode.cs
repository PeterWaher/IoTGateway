using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Abstract base class for tree nodes in the connection view.
	/// </summary>
	public abstract class TreeNode : IDisposable
	{
		private TreeNode parent;
		private TreeNode[] children = null;
		private string Text;
		private object tag = null;

		/// <summary>
		/// Abstract base class for tree nodes in the connection view.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		public TreeNode(TreeNode Parent)
		{
			this.parent = Parent;
		}

		/// <summary>
		/// If the node has child nodes or not. If null, the state is undefined, and might need to be checked by consulting with the
		/// back-end service corresponding to the node.
		/// </summary>
		public bool? HasChildren
		{
			get
			{
				if (this.children == null)
					return null;
				else
					return this.children.Length > 0;
			}
		}

		/// <summary>
		/// Children of the node. If null, children are not loaded.
		/// </summary>
		public TreeNode[] Children
		{
			get { return this.children; }
		}

		/// <summary>
		/// Parent node. May be null if a root node.
		/// </summary>
		public TreeNode Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Object tagged to the node.
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		/// <summary>
		/// Tree Node header text.
		/// </summary>
		public abstract string Header
		{
			get;
		}

		/// <summary>
		/// Disposes of the node and its resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Saves the object to a file.
		/// </summary>
		/// <param name="Output">Output</param>
		public abstract void Write(XmlWriter Output);
	}
}
