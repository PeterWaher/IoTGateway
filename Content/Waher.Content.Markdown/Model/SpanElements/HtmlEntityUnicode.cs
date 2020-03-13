using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an HTML entity in Unicode format.
	/// </summary>
	public class HtmlEntityUnicode : MarkdownElement
	{
		private readonly int code;

		/// <summary>
		/// Represents an HTML entity in Unicode format.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Code">HTML Entity.</param>
		public HtmlEntityUnicode(MarkdownDocument Document, int Code)
			: base(Document)
		{
			this.code = Code;
		}

		/// <summary>
		/// Unicode character
		/// </summary>
		public int Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			this.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("&#");
			Output.Append(this.code.ToString());
			Output.Append(';');
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append((char)this.code);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return new string((char)this.code, 1);
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteValue(new string((char)this.code, 1));
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
			Output.WriteElementString("HtmlEntityUnicode", this.code.ToString());
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is HtmlEntityUnicode x &&
				this.code == x.code &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.code.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
