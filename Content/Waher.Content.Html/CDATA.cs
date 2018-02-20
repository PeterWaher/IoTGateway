using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// CDATA content.
	/// </summary>
	public class CDATA : HtmlNode
    {
		private string content;

		/// <summary>
		/// CDATA content.
		/// </summary>
		/// <param name="Parent">Parent node. Can be null for root elements.</param>
		/// <param name="Content">CDATA Content</param>
		public CDATA(HtmlNode Parent, string Content)
			: base(Parent)
		{
			this.content = Content;
		}

		/// <summary>
		/// CDATA Content
		/// </summary>
		public string Content
		{
			get { return this.content; }
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteCData(this.content);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.content;
		}
	}
}
