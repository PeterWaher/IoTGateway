using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML attribute
	/// </summary>
	public class HtmlAttribute : HtmlNode
	{
		private LinkedList<HtmlNode> segments = null;
		private readonly string name;
		private string value;

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
			this.name = Name;
			this.value = Value;
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
			this.name = Name;
			this.value = null;
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
		public HtmlAttribute(HtmlDocument Document, HtmlElement Parent, int StartPosition, string Name, string Value)
			: base(Document, Parent, StartPosition)
		{
			this.name = Name;
			this.value = Value;
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
			this.name = Name;
			this.value = null;
			this.segments = null;
		}

		/// <summary>
		/// Attribute name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		internal void Add(HtmlNode Segment)
		{
			if (this.segments is null)
				this.segments = new LinkedList<HtmlNode>();

			this.segments.AddLast(Segment);
			this.value = null;
		}

		internal bool HasSegments
		{
			get { return this.segments != null; }
		}

		/// <summary>
		/// Attribute value.
		/// </summary>
		public string Value
		{
			get
			{
				if (this.value is null)
				{
					if (!(this.segments is null))
					{
						StringBuilder sb = new StringBuilder();

						foreach (HtmlNode N in this.segments)
							sb.Append(N.ToString());

						this.value = sb.ToString();
					}
					else
						this.value = string.Empty;
				}

				return this.value;
			}

			internal set
			{
				this.value = value;
				this.segments = null;
			}
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name + "=" + this.Value;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteAttributeString(this.name, this.value);
		}

        /// <summary>
        /// Exports the HTML document to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        public override void Export(StringBuilder Output)
        {
            Output.Append(' ');
            Output.Append(this.name);
            Output.Append("=\"");
            Output.Append(Xml.XML.Encode(this.value));
            Output.Append('"');
        }
    }
}
