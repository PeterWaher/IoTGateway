using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a paragraph in a markdown document.
	/// </summary>
	public class Paragraph : MarkdownElementChildren
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
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
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

					E.GenerateXAML(Output, Settings, TextAlignment);

					Output.WriteEndElement();
				}
				else
					E.GenerateXAML(Output, Settings, TextAlignment);
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
	}
}
