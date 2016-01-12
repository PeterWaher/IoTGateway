using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a block quote in a markdown document.
	/// </summary>
	public class BlockQuote : MarkdownElementChildren
	{
		/// <summary>
		/// Represents a block quote in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public BlockQuote(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<blockquote>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.AppendLine("</blockquote>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();

			foreach (MarkdownElement E in this.Children)
				E.GeneratePlainText(sb);

			string s = sb.ToString().Trim();
			s = s.Replace("\r\n", "\n").Replace("\r", "\n");

			foreach (string Row in s.Split('\n'))
			{
				Output.Append("> ");
				Output.AppendLine(Row);
			}

			Output.AppendLine();
		}
	}
}
