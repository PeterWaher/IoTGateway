using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Strong text
	/// </summary>
	public class Strong : MarkdownElementChildren
	{
		/// <summary>
		/// Strong text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		public Strong(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<strong>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</strong>");
		}
	
	}
}
