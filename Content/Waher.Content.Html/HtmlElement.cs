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
		private string name;

		/// <summary>
		/// Base class for all HTML elements.
		/// </summary>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="Name">Tag name.</param>
		public HtmlElement(HtmlElement Parent, string Name)
			: base(Parent)
		{
			this.name = Name;
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		internal void Add(HtmlNode Node)
		{
			if (this.children == null)
				this.children = new LinkedList<HtmlNode>();

			this.children.AddLast(Node);
		}

		internal void AddAttribute(HtmlAttribute Attribute)
		{
			if (this.attributes == null)
				this.attributes = new LinkedList<HtmlAttribute>();

			this.attributes.AddLast(Attribute);
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
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement(this.name);

			if (this.attributes != null)
			{
				foreach (HtmlAttribute Attr in this.attributes)
					Attr.Export(Output);
			}

			if (this.children != null)
			{
				foreach (HtmlNode Child in this.children)
					Child.Export(Output);
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an empty element.
		/// </summary>
		public virtual bool IsEmptyElement
		{
			get { return false; }
		}
	}
}
