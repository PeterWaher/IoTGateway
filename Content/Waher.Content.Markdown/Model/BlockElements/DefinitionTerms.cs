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
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				E.GenerateMarkdown(Output);
				Output.AppendLine();
			}
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
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			int TopMargin = Settings.ParagraphMarginTop;

			foreach (MarkdownElement Term in this.Children)
			{
				Output.WriteStartElement("TextBlock");
				Output.WriteAttributeString("TextWrapping", "Wrap");
				Output.WriteAttributeString("Margin", Settings.ParagraphMarginLeft.ToString() + "," + TopMargin.ToString() + "," +
					Settings.ParagraphMarginRight.ToString() + ",0");
				Output.WriteAttributeString("FontWeight", "Bold");

				Term.GenerateXAML(Output, TextAlignment);

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

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new DefinitionTerms(Document, Children);
		}

	}
}
