using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Horizontal rule
	/// </summary>
	public class HorizontalRule : MarkdownElement
	{
		/// <summary>
		/// Horizontal rule
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public HorizontalRule(MarkdownDocument Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<hr/>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.AppendLine(new string('-', 80));
		}
	
	}
}
