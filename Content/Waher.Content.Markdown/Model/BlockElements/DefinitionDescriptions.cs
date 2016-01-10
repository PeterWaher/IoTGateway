using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition descriptions
	/// </summary>
	public class DefinitionDescriptions : MarkdownElementChildren
	{
		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, IEnumerable<MarkdownElement> Descriptions)
			: base(Document, Descriptions)
		{
		}

		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, params MarkdownElement[] Descriptions)
			: base(Document, Descriptions)
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
				Output.Append("<dd>");
				E.GenerateHTML(Output);
				Output.AppendLine("</dd>");
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
				Output.Append(":   ");
				E.GeneratePlainText(Output);
				Output.AppendLine();
			}

			Output.AppendLine();
		}
	
	}
}
