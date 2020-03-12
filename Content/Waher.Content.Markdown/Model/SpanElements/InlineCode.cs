using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineCode : MarkdownElement
	{
		private readonly string code;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Code">Inline Code.</param>
		public InlineCode(MarkdownDocument Document, string Code)
			: base(Document)
		{
			if (Code.StartsWith(" "))
				Code = Code.Substring(1);

			if (Code.EndsWith(" "))
				Code = Code.Substring(0, Code.Length - 1);

			this.code = Code;
		}

		/// <summary>
		/// Inline code.
		/// </summary>
		public string Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<code>");
			Output.Append(XML.HtmlValueEncode(this.code));
			Output.Append("</code>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.code);
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("xml", "space", null, "preserve");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			Output.WriteAttributeString("FontFamily", "Courier New");
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			Output.WriteValue(this.code);

			Output.WriteEndElement();
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
			Output.WriteStartElement("InlineCode");
			Output.WriteCData(this.code);
			Output.WriteEndElement();
		}

	}
}
