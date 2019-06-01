using System;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
    /// <summary>
    /// Base class for all HTML nodes.
    /// </summary>
    public abstract class HtmlNode
    {
        private readonly HtmlDocument document;
        private readonly HtmlNode parent;
        private int start;
        private int end;

        /// <summary>
        /// Base class for all HTML nodes.
        /// </summary>
        /// <param name="Document">HTML Document.</param>
        /// <param name="Parent">Parent node. Can be null for root elements.</param>
        /// <param name="StartPosition">Start position.</param>
        public HtmlNode(HtmlDocument Document, HtmlNode Parent, int StartPosition)
        {
            this.document = Document;
            this.parent = Parent;
            this.start = StartPosition;
            this.end = -1;
        }

        /// <summary>
        /// Base class for all HTML nodes.
        /// </summary>
        /// <param name="Document">HTML Document.</param>
        /// <param name="Parent">Parent node. Can be null for root elements.</param>
        /// <param name="StartPosition">Start position.</param>
        /// <param name="EndPosition">End position.</param>
        public HtmlNode(HtmlDocument Document, HtmlNode Parent, int StartPosition, int EndPosition)
        {
            this.document = Document;
            this.parent = Parent;
            this.start = StartPosition;
            this.end = EndPosition;
        }

        /// <summary>
        /// HTML Document
        /// </summary>
        public HtmlDocument Document
        {
            get { return this.document; }
        }

        /// <summary>
        /// Parent node, if available.
        /// </summary>
        public HtmlNode Parent
        {
            get { return this.parent; }
        }

        /// <summary>
        /// Start position of element.
        /// </summary>
        public int StartPosition
        {
            get { return this.start; }
            internal set { this.start = value; }
        }

        /// <summary>
        /// End position of element.
        /// </summary>
        public int EndPosition
        {
            get { return this.end; }
            internal set { this.end = value; }
        }

        /// <summary>
        /// Outer HTML
        /// </summary>
        public string OuterHtml
        {
            get
            {
                return this.document.HtmlText.Substring(this.start, this.end - this.start + 1);
            }
        }

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public abstract void Export(XmlWriter Output);

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public abstract void Export(StringBuilder Output);
    }
}
