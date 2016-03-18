using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Section Separator
	/// </summary>
	public class SectionSeparator : MarkdownElement
	{
		private int sectionNr;
		private int nrColumns;

		/// <summary>
		/// Section Separator
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="SectionNr">Section number</param>
		/// <param name="NrColumns">Number of columns in following section.</param>
		public SectionSeparator(MarkdownDocument Document, int SectionNr, int NrColumns)
			: base(Document)
		{
			this.sectionNr = SectionNr;
			this.nrColumns = NrColumns;
		}

		/// <summary>
		/// Section number.
		/// </summary>
		public int SectionNr
		{
			get { return this.sectionNr; }
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
			Output.AppendLine("</section>");
			GenerateSectionHTML(Output, this.nrColumns);
		}

		internal static void GenerateSectionHTML(StringBuilder Output, int NrColumns)
		{
			Output.Append("<section");

			if (NrColumns > 1)
			{
				string s = NrColumns.ToString();

				Output.Append(" style=\"-webkit-column-count:");
				Output.Append(s);
				Output.Append(";-moz-column-count:");
				Output.Append(s);
				Output.Append(";column-count:");
				Output.Append(s);
				Output.Append('"');
			}

			Output.AppendLine(">");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.AppendLine(new string('=', 80));
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
			Output.WriteElementString("Separator", string.Empty);
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
