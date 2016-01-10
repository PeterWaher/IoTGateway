using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a definition list in a markdown document.
	/// </summary>
	public class DefinitionList : MarkdownElementChildren
	{
		/// <summary>
		/// Represents a definition list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public DefinitionList(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a definition list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public DefinitionList(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<dl>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.AppendLine("</dl>");
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
		}

		internal override bool OutsideParagraph
		{
			get
			{
				return true;
			}
		}

		internal override bool JoinOverParagraphs
		{
			get
			{
				return true;
			}
		}

	}
}
