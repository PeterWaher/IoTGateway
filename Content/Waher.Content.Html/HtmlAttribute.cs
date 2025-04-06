using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML attribute
	/// </summary>
	public class HtmlAttribute : HtmlNode
	{
		private ChunkedList<HtmlNode> segments = null;
		private readonly string fullName;
		private readonly string localName;
		private readonly string prefix;
		private readonly bool hasPrefix;
		private string @value;

		/// <summary>
		/// HTML attribute
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Element to whom the attribute belongs.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Name">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public HtmlAttribute(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			int EndPosition, string Name, string Value)
			: base(Document, Parent, StartPosition, EndPosition)
		{
			SplitName(Name, out this.prefix, out this.localName, out this.hasPrefix);
			this.fullName = Name;
			this.@value = Value;
			this.segments = null;
		}

		/// <summary>
		/// HTML attribute
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Element to whom the attribute belongs.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Name">Attribute name.</param>
		public HtmlAttribute(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			int EndPosition, string Name)
			: base(Document, Parent, StartPosition, EndPosition)
		{
			SplitName(Name, out this.prefix, out this.localName, out this.hasPrefix);
			this.fullName = Name;
			this.@value = null;
			this.segments = null;
		}

		/// <summary>
		/// HTML attribute
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Element to whom the attribute belongs.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="Name">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public HtmlAttribute(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			string Name, string Value)
			: base(Document, Parent, StartPosition)
		{
			SplitName(Name, out this.prefix, out this.localName, out this.hasPrefix);
			this.fullName = Name;
			this.@value = Value;
			this.segments = null;
		}

		/// <summary>
		/// HTML attribute
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Element to whom the attribute belongs.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="Name">Attribute name.</param>
		public HtmlAttribute(HtmlDocument Document, HtmlElement Parent, int StartPosition, string Name)
			: base(Document, Parent, StartPosition)
		{
			SplitName(Name, out this.prefix, out this.localName, out this.hasPrefix);
			this.fullName = Name;
			this.@value = null;
			this.segments = null;
		}

		/// <summary>
		/// Attribute full name (including prefix).
		/// </summary>
		public string FullName => this.fullName;

		/// <summary>
		/// Attribute local name.
		/// </summary>
		public string LocalName => this.localName;

		/// <summary>
		/// Attribute prefix.
		/// </summary>
		public string Prefix => this.prefix;

		/// <summary>
		/// If the attribute has a prefix.
		/// </summary>
		public bool HasPrefix => this.hasPrefix;

		internal void Add(HtmlNode Segment)
		{
			if (this.segments is null)
				this.segments = new ChunkedList<HtmlNode>();

			this.segments.Add(Segment);
			this.@value = null;
		}

		internal bool HasSegments => !(this.segments is null);

		/// <summary>
		/// Attribute value.
		/// </summary>
		public string Value
		{
			get
			{
				if (this.@value is null)
				{
					if (!(this.segments is null))
					{
						StringBuilder sb = new StringBuilder();

						foreach (HtmlNode N in this.segments)
							sb.Append(N.ToString());

						this.@value = sb.ToString();
					}
					else
						this.@value = string.Empty;
				}

				return this.@value;
			}

			internal set
			{
				this.@value = value;
				this.segments = null;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.hasPrefix)
				return this.prefix + ":" + this.localName + "=" + this.Value;
			else
				return this.localName + "=" + this.Value;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="Namespaces">Namespaces defined, by prefix.</param>
		public override void Export(XmlWriter Output, Dictionary<string, string> Namespaces)
		{
			if (this.hasPrefix)
			{
				if (Namespaces.TryGetValue(this.prefix, out string Namespace))
					Output.WriteAttributeString(this.prefix, this.localName, Namespace, this.@value);
			}
			else
				Output.WriteAttributeString(this.localName, this.@value);
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(StringBuilder Output)
		{
			Output.Append(' ');
			Output.Append(this.fullName);
			Output.Append("=\"");
			Output.Append(Xml.XML.Encode(this.@value));
			Output.Append('"');
		}
	}
}
