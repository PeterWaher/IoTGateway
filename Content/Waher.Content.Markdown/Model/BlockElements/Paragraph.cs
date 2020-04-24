using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a paragraph in a markdown document.
	/// </summary>
	public class Paragraph : BlockElementChildren
	{
		/// <summary>
		/// Represents a paragraph in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public Paragraph(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			base.GenerateMarkdown(Output);
			Output.AppendLine();
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<p>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.AppendLine("</p>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GeneratePlainText(Output);

			Output.AppendLine();
			Output.AppendLine();
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			string s;

			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			foreach (MarkdownElement E in this.Children)
			{
				if ((!E.InlineSpanElement || E.OutsideParagraph) && (s = E.BaselineAlignment) != "Baseline")
				{
					Output.WriteStartElement("InlineUIContainer");
					Output.WriteAttributeString("BaselineAlignment", s);

					E.GenerateXAML(Output, TextAlignment);

					Output.WriteEndElement();
				}
				else
					E.GenerateXAML(Output, TextAlignment);
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "Paragraph");
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Paragraph(Document, Children);
		}

	}
}
