using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Base class for all HTML nodes.
	/// </summary>
    public abstract class HtmlNode
    {
		private HtmlNode parent;

		/// <summary>
		/// Base class for all HTML nodes.
		/// </summary>
		/// <param name="Parent">Parent node. Can be null for root elements.</param>
		public HtmlNode(HtmlNode Parent)
		{
			this.parent = Parent;
		}

		/// <summary>
		/// Parent node, if available.
		/// </summary>
		public HtmlNode Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public abstract void Export(XmlWriter Output);
    }
}
