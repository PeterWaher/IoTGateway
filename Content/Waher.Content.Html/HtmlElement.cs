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
		private Dictionary<string, HtmlAttribute> attributesByName = null;
		private LinkedList<HtmlAttribute> attributes = null;
		private Dictionary<string, HtmlAttribute> namespacesByPrefix = null;
		private LinkedList<HtmlNode> children = null;
		private readonly string fullName;
		private readonly string localName;
		private readonly string prefix;
		private readonly bool hasPrefix;
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
			SplitName(Name, out this.prefix, out this.localName, out this.hasPrefix);
			this.fullName = Name;
			this.@namespace = null;
		}

		/// <summary>
		/// Element full name (including prefix).
		/// </summary>
		public string FullName => this.fullName;

		/// <summary>
		/// Element local name.
		/// </summary>
		public string LocalName => this.localName;

		/// <summary>
		/// Element prefix.
		/// </summary>
		public string Prefix => this.prefix;

		/// <summary>
		/// If the element has a prefix.
		/// </summary>
		public bool HasPrefix => this.hasPrefix;

		/// <summary>
		/// Namespace, if provided, null if not.
		/// </summary>
		public string Namespace => this.@namespace;

		internal void Add(HtmlNode Node)
		{
			if (this.children is null)
				this.children = new LinkedList<HtmlNode>();

			this.children.AddLast(Node);
		}

		internal void AddAttribute(HtmlAttribute Attribute)
		{
			if (!Attribute.HasPrefix && Attribute.LocalName == "xmlns")
			{
				if (string.IsNullOrEmpty(this.@namespace))
					this.@namespace = Attribute.Value;
			}
			else if (Attribute.HasPrefix && Attribute.Prefix == "xmlns")
			{
				if (this.namespacesByPrefix is null)
					this.namespacesByPrefix = new Dictionary<string, HtmlAttribute>();

				this.namespacesByPrefix[Attribute.LocalName] = Attribute;
			}
			else
			{
				if (this.attributes is null)
				{
					this.attributes = new LinkedList<HtmlAttribute>();
					this.attributesByName = new Dictionary<string, HtmlAttribute>(StringComparer.OrdinalIgnoreCase);
				}

				if (!this.attributesByName.ContainsKey(Attribute.FullName))
				{
					this.attributes.AddLast(Attribute);
					this.attributesByName[Attribute.FullName] = Attribute;
				}
			}
		}

		/// <summary>
		/// If the element has children.
		/// </summary>
		public bool HasChildren => !(this.children is null);

		/// <summary>
		/// If the element has attributes.
		/// </summary>
		public bool HasAttributes => !(this.attributes is null);

		/// <summary>
		/// Child nodes, or null if none.
		/// </summary>
		public IEnumerable<HtmlNode> Children => this.children;

		/// <summary>
		/// Attributes, or null if none.
		/// </summary>
		public IEnumerable<HtmlAttribute> Attributes => this.attributes;

		/// <summary>
		/// Tries to get an attribute from the element, by its name.
		/// </summary>
		/// <param name="Name">Attribute name.</param>
		/// <param name="Attribute">Attribute object, if found, null otherwise.</param>
		/// <returns>If attribute was found.</returns>
		public bool TryGetAttribute(string Name, out HtmlAttribute Attribute)
		{
			if (this.attributesByName is null)
			{
				Attribute = null;
				return false;
			}
			else
				return this.attributesByName.TryGetValue(Name, out Attribute);
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

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.StartTag;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="Namespaces">Namespaces defined, by prefix.</param>
		public override void Export(XmlWriter Output, Dictionary<string, string> Namespaces)
		{
			LinkedList<KeyValuePair<string, string>> NamespaceBak = null;

			if (!(this.namespacesByPrefix is null))
			{
				foreach (KeyValuePair<string, HtmlAttribute> P in this.namespacesByPrefix)
				{
					if (Namespaces.TryGetValue(P.Key, out string s))
					{
						if (NamespaceBak is null)
							NamespaceBak = new LinkedList<KeyValuePair<string, string>>();

						NamespaceBak.AddLast(new KeyValuePair<string, string>(P.Key, s));
					}

					Namespaces[P.Key] = P.Value.Value;
				}
			}

			if (this.hasPrefix)
			{
				if (Namespaces.TryGetValue(this.prefix, out string s))
					Output.WriteStartElement(this.prefix, this.localName, s);
				else
					Output.WriteStartElement(this.prefix, this.localName, this.@namespace);
			}
			else if (this.@namespace is null)
				Output.WriteStartElement(this.localName);
			else
				Output.WriteStartElement(this.localName, this.@namespace);

			if (!(this.attributes is null))
			{
				foreach (HtmlAttribute Attr in this.attributes)
					Attr.Export(Output, Namespaces);
			}

			if (!(this.namespacesByPrefix is null))
			{
				foreach (KeyValuePair<string, HtmlAttribute> P in this.namespacesByPrefix)
					P.Value.Export(Output, Namespaces);
			}

			if (!(this.children is null))
			{
				foreach (HtmlNode Child in this.children)
					Child.Export(Output, Namespaces);
			}

			Output.WriteEndElement();

			if (!(this.namespacesByPrefix is null))
			{
				foreach (KeyValuePair<string, HtmlAttribute> P in this.namespacesByPrefix)
					Namespaces.Remove(P.Key);
			}

			if (!(NamespaceBak is null))
			{
				foreach (KeyValuePair<string, string> P in NamespaceBak)
					Namespaces[P.Key] = P.Value;
			}
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(StringBuilder Output)
		{
			Output.Append('<');
			Output.Append(this.fullName);

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

			if (!(this.namespacesByPrefix is null))
			{
				foreach (KeyValuePair<string, HtmlAttribute> P in this.namespacesByPrefix)
					P.Value.Export(Output);
			}

			if (this.children is null)
				Output.Append("/>");
			else
			{
				Output.Append('>');

				foreach (HtmlNode Child in this.children)
					Child.Export(Output);

				Output.Append("</");
				Output.Append(this.fullName);
				Output.Append('>');
			}
		}

		/// <summary>
		/// If the element is an empty element.
		/// </summary>
		public virtual bool IsEmptyElement => false;

		/// <summary>
		/// If the element has an attribute with a given (case-insensitive) name.
		/// </summary>
		/// <param name="Name">Case-insensitive attribute name.</param>
		/// <returns>If such an attribute exists.</returns>
		public bool HasAttribute(string Name)
		{
			if (Name.StartsWith("xmlns"))
			{
				if (Name == "xmlns")
					return !(this.@namespace is null);

				if (Name.Length > 6 && Name[5] == ':')
				{
					if (this.namespacesByPrefix is null)
						return false;

					return this.namespacesByPrefix.ContainsKey(Name.Substring(6));
				}
			}

			if (this.attributes is null)
				return false;

			foreach (HtmlAttribute Attr in this.attributes)
			{
				if (string.Compare(Attr.FullName, Name, true) == 0)
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
			if (Name.StartsWith("xmlns"))
			{
				if (Name == "xmlns")
					return this.@namespace;

				if (Name.Length > 6 && Name[5] == ':')
				{
					if (this.namespacesByPrefix is null)
						return string.Empty;

					if (this.namespacesByPrefix.TryGetValue(Name.Substring(6), out HtmlAttribute NsAttr))
						return NsAttr.Value;
					else
						return string.Empty;
				}
			}

			if (this.attributesByName is null)
				return string.Empty;

			if (this.attributesByName.TryGetValue(Name, out HtmlAttribute Attr))
				return Attr.Value;
			else
				return string.Empty;
		}

		/// <summary>
		/// Gets a given attribute value, if available. The empty string is returned if the attribute does not exist.
		/// </summary>
		/// <param name="Name">Name of attribute.</param>
		/// <returns>Attribute value, if exists, empty string otherwise.</returns>
		public string this[string Name] => this.GetAttribute(Name);
	}
}
