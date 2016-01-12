using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a block of HTML in a markdown document.
	/// </summary>
	public class HtmlBlock : MarkdownElementChildren
	{
		/// <summary>
		/// Represents a block of HTML in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public HtmlBlock(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);
	
			Output.AppendLine();
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			string s;
			bool First = true;

			foreach (MarkdownElement E in this.Children)
			{
				E.GeneratePlainText(sb);
				s = sb.ToString().TrimStart().Trim(' ');	// Only space at the end, not CRLF
				sb.Clear();

				if (!string.IsNullOrEmpty(s))
				{
					if (First)
						First = false;
					else
						Output.Append(' ');

					Output.Append(s);

					if (s.EndsWith("\n"))
						First = true;
				}
			}

			Output.AppendLine();
			Output.AppendLine();
		}
	}
}
