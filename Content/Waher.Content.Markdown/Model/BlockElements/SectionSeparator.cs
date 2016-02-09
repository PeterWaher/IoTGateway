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

		/// <summary>
		/// Section Separator
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public SectionSeparator(MarkdownDocument Document, int SectionNr)
			: base(Document)
		{
			this.sectionNr = SectionNr;
		}

		/// <summary>
		/// Section number.
		/// </summary>
		public int SectionNr
		{
			get { return this.sectionNr; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("</section>");
			Output.AppendLine("<section>");
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
