using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition terms
	/// </summary>
	public class DefinitionTerms : BlockElementChildren
	{
		/// <summary>
		/// Definition terms
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Terms">Terms</param>
		public DefinitionTerms(MarkdownDocument Document, IEnumerable<MarkdownElement> Terms)
			: base(Document, Terms)
		{
		}

		/// <summary>
		/// Definition terms
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Terms">Terms</param>
		public DefinitionTerms(MarkdownDocument Document, params MarkdownElement[] Terms)
			: base(Document, Terms)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				Output.Append("<dt>");
				E.GenerateHTML(Output);
				Output.AppendLine("</dt>");
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			string s;

			foreach (MarkdownElement E in this.Children)
			{
				E.GeneratePlainText(sb);
				s = sb.ToString();
				sb.Clear();
				Output.Append(s);

				if (!s.EndsWith(Environment.NewLine))
					Output.AppendLine();
			}
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			int TopMargin = Settings.ParagraphMarginTop;

			foreach (MarkdownElement Term in this.Children)
			{
				Output.WriteStartElement("TextBlock");
				Output.WriteAttributeString("TextWrapping", "Wrap");
				Output.WriteAttributeString("Margin", Settings.ParagraphMarginLeft.ToString() + "," + TopMargin.ToString() + "," +
					Settings.ParagraphMarginRight.ToString() + ",0");
				Output.WriteAttributeString("FontWeight", "Bold");

				Term.GenerateXAML(Output, Settings, TextAlignment);

				Output.WriteEndElement();
				TopMargin = 0;
			}
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
			this.Export(Output, "DefinitionTerms");
		}

	}
}
