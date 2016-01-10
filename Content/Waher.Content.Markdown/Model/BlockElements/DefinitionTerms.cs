using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition terms
	/// </summary>
	public class DefinitionTerms : MarkdownElementChildren
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
			foreach (MarkdownElement E in this.Children)
			{
				E.GeneratePlainText(Output);
				Output.AppendLine();
			}
		}
	
	}
}
