using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Underlined text
	/// </summary>
	public class Underline : MarkdownElementChildren
	{
		/// <summary>
		/// Underlined text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		public Underline(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<u>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</u>");
		}
	
	}
}
