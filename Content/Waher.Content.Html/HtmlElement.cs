using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Base class for all HTML elements.
	/// </summary>
	public class HtmlElement : HtmlNode
	{
		private LinkedList<HtmlAttribute> attributes = null;
		private LinkedList<HtmlNode> children = null;
		private readonly string name;
		private string @namespace;

		/// <summary>
		/// Base class for all HTML elements.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="Name">Tag name.</param>
		public HtmlElement(HtmlDocument Document, HtmlElement Parent, int StartPosition, string Name)
			: base(Document, Parent, StartPosition)
		{
			this.name = Name;
			this.@namespace = null;
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Namespace, if provided, null if not.
		/// </summary>
		public string Namespace
		{
			get { return this.@namespace; }
		}

		internal void Add(HtmlNode Node)
		{
			if (this.children is null)
				this.children = new LinkedList<HtmlNode>();

			this.children.AddLast(Node);
		}

		internal void AddAttribute(HtmlAttribute Attribute)
		{
			if (Attribute.Name == "xmlns")
				this.@namespace = Attribute.Value;
			else
			{
				if (this.attributes is null)
					this.attributes = new LinkedList<HtmlAttribute>();

				this.attributes.AddLast(Attribute);
			}
		}

		/// <summary>
		/// If the element has children.
		/// </summary>
		public bool HasChildren
		{
			get { return this.children != null; }
		}

		/// <summary>
		/// If the element has attributes.
		/// </summary>
		public bool HasAttributes
		{
			get { return this.attributes != null; }
		}

		/// <summary>
		/// Child nodes, or null if none.
		/// </summary>
		public IEnumerable<HtmlNode> Children
		{
			get { return this.children; }
		}

		/// <summary>
		/// Attributes, or null if none.
		/// </summary>
		public IEnumerable<HtmlAttribute> Attributes
		{
			get { return this.attributes; }
		}

		/// <summary>
		/// Inner HTML
		/// </summary>
		public string InnerHtml
		{
			get
			{
				int? Start = null;
				int? End = null;

				if (!(this.children is null))
				{
					foreach (HtmlNode N in this.children)
					{
						if (!Start.HasValue)
							Start = N.StartPosition;

						End = N.EndPosition;
					}
				}

				if (Start.HasValue && End.HasValue)
					return this.Document.HtmlText.Substring(Start.Value, End.Value - Start.Value + 1);
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Inner Text
		/// </summary>
		public string InnerText
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				if (!(this.children is null))
				{
					foreach (HtmlNode N in this.children)
					{
						if (N is HtmlText Text)
							sb.Append(Text.InlineText);
						else if (N is HtmlElement Element)
							sb.Append(Element.InnerText);
						else if (N is HtmlEntity Entity)
							sb.Append(Entity.ToString());
						else if (N is CDATA CDATA)
							sb.Append(CDATA.Content);
					}
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Start tag.
		/// </summary>
		public string StartTag
		{
			get
			{
				if (!(this.children is null))
				{
					foreach (HtmlNode N in this.children)
						return this.Document.HtmlText.Substring(this.StartPosition, N.StartPosition - this.StartPosition);
				}

				return this.OuterHtml;
			}
		}

		/// <summary>
		/// End position of start tag.
		/// </summary>
		public int EndPositionOfStartTag
		{
			get
			{
				if (!(this.children is null))
				{
					foreach (HtmlNode N in this.children)
						return N.StartPosition - 1;
				}

				return this.EndPosition;
			}
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.StartTag;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			if (this.@namespace is null)
				Output.WriteStartElement(this.name);
			else
				Output.WriteStartElement(this.name, this.@namespace);

			if (!(this.attributes is null))
			{
				foreach (HtmlAttribute Attr in this.attributes)
					Attr.Export(Output);
			}

			if (!(this.children is null))
			{
				foreach (HtmlNode Child in this.children)
					Child.Export(Output);
			}

			Output.WriteEndElement();
		}

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public override void Export(StringBuilder Output)
        {
            Output.Append('<');
            Output.Append(this.name);

            if (!(this.attributes is null))
            {
                foreach (HtmlAttribute Attr in this.attributes)
                    Attr.Export(Output);
            }

			if (!(this.@namespace is null))
			{
				Output.Append(" xmlns=\"");
				Output.Append(Xml.XML.Encode(this.@namespace));
				Output.Append('"');
			}

            if (this.children is null)
                Output.Append("/>");
            else
            {
                Output.Append('>');

                foreach (HtmlNode Child in this.children)
                    Child.Export(Output);

                Output.Append("</");
                Output.Append(this.name);
                Output.Append('>');
            }
        }

        /// <summary>
        /// If the element is an empty element.
        /// </summary>
        public virtual bool IsEmptyElement
		{
			get { return false; }
		}

		/// <summary>
		/// If the element has an attribute with a given (case-insensitive) name.
		/// </summary>
		/// <param name="Name">Case-insensitive attribute name.</param>
		/// <returns>If such an attribute exists.</returns>
		public bool HasAttribute(string Name)
		{
			if (Name == "xmlns")
				return !(this.@namespace is null);

			if (this.attributes is null)
				return false;

			foreach (HtmlAttribute Attr in this.attributes)
			{
				if (string.Compare(Attr.Name, Name, true) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets a given attribute value, if available. The empty string is returned if the attribute does not exist.
		/// </summary>
		/// <param name="Name">Case-insensitive attribute name.</param>
		/// <returns>Attribute value.</returns>
		public string GetAttribute(string Name)
		{
			if (Name == "xmlns")
				return this.@namespace;

			if (this.attributes is null)
				return string.Empty;

			foreach (HtmlAttribute Attr in this.attributes)
			{
				if (string.Compare(Attr.Name, Name, true) == 0)
					return Attr.Value;
			}

			return string.Empty;
		}
	}
}
