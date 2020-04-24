using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents a hashtag.
	/// </summary>
	public class HashTag : MarkdownElement
	{
		private readonly string tag;

		/// <summary>
		/// Represents a hashtag.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Tag">Hashtag.</param>
		public HashTag(MarkdownDocument Document, string Tag)
			: base(Document)
		{
			this.tag = Tag;
		}

		/// <summary>
		/// Hashtag
		/// </summary>
		public string Tag
		{
			get { return this.tag; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('#');
			Output.Append(this.tag);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<mark");

			HtmlSettings Settings = this.Document.Settings?.HtmlSettings;
			string s = Settings?.HashtagClass;

			if (!string.IsNullOrEmpty(s))
			{
				Output.Append(" class=\"");
				Output.Append(XML.HtmlAttributeEncode(s));
				Output.Append('"');
			}

			s = Settings?.HashtagClickScript;

			if (!string.IsNullOrEmpty(s))
			{
				Output.Append(" onclick=\"");
				Output.Append(XML.HtmlAttributeEncode(s));
				Output.Append('"');
			}

			Output.Append('>');
			Output.Append(this.tag);
			Output.Append("</mark>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.tag);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "#" + this.tag;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteValue(this.tag);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteElementString("Hashtag", this.tag);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is HashTag x &&
				this.tag == x.tag &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.tag?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
