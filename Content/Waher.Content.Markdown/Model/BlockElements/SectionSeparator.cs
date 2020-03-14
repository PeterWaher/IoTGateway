using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Section Separator
	/// </summary>
	public class SectionSeparator : BlockElement
	{
		private readonly string row;
		private readonly int sectionNr;
		private readonly int nrColumns;

		/// <summary>
		/// Section Separator
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="SectionNr">Section number</param>
		/// <param name="NrColumns">Number of columns in following section.</param>
		/// <param name="Row">Markdown definition.</param>
		public SectionSeparator(MarkdownDocument Document, int SectionNr, int NrColumns, string Row)
			: base(Document)
		{
			this.sectionNr = SectionNr;
			this.nrColumns = NrColumns;
			this.row = Row;
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
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.AppendLine(this.row);
			Output.AppendLine();
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

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("SectionSeparator");
			Output.WriteAttributeString("sectionNr", this.sectionNr.ToString());
			Output.WriteAttributeString("nrColumns", this.nrColumns.ToString());
			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is SectionSeparator x &&
				this.nrColumns == x.nrColumns &&
				this.sectionNr == x.sectionNr &&
				this.row == x.row &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.row?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.nrColumns.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.sectionNr.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
