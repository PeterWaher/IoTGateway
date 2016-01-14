using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Unformatted text.
	/// </summary>
	public class InlineText : MarkdownElement
	{
		private string value;

		/// <summary>
		/// Unformatted text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Value">Inline text.</param>
		public InlineText(MarkdownDocument Document, string Value)
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Unformatted text.
		/// </summary>
		public string Value
		{
			get { return this.value; }
			internal set { this.value = value; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append(MarkdownDocument.HtmlValueEncode(this.value));
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.value);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteValue(this.value);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}
	}
}
