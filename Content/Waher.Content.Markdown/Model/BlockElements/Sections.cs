using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a sequence of sections.
	/// </summary>
	public class Sections : MarkdownElementChildren
	{
		private int nrColumns;

		/// <summary>
		/// Represents a sequence of sections.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="NrColumns">Number of columns in first section.</param>
		/// <param name="Children">Child elements.</param>
		public Sections(MarkdownDocument Document, int NrColumns, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.nrColumns = NrColumns;
		}

		/// <summary>
		/// Number of columns in following section.
		/// </summary>
		public int NrColumns
		{
			get { return this.nrColumns; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			SectionSeparator.GenerateSectionHTML(Output, this.nrColumns);

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.AppendLine("</section>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GeneratePlainText(Output);
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateXAML(Output, Settings, TextAlignment);
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
