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
		private readonly string content;

		/// <summary>
		/// CDATA content.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Content">CDATA Content</param>
		public CDATA(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			int EndPosition, string Content)
			: base(Document, Parent, StartPosition, EndPosition)
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
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public override void Export(StringBuilder Output)
        {
            Output.Append("<![CDATA[");
            Output.Append(this.content);
            Output.Append("]]>");
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
