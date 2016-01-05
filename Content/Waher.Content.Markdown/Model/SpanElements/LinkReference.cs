using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link reference
	/// </summary>
	public class LinkReference : MarkdownElementChildren
	{
		private string label;

		/// <summary>
		/// Link
		/// </summary>
		public LinkReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label)
			: base(Document, ChildElements)
		{
			this.label = Label;
		}

		/// <summary>
		/// Link label
		/// </summary>
		private string Label
		{
			get { return this.label; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Link Link = this.Document.GetLinkReference(this.label);

			if (Link != null)
			{
				Output.Append("<a href=\"");
				Output.Append(MarkdownDocument.HtmlEncode(Link.Url));

				if (!string.IsNullOrEmpty(Link.Title))
				{
					Output.Append("\" title=\"");
					Output.Append(MarkdownDocument.HtmlEncode(Link.Title));
				}

				Output.Append("\">");
			}

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			if (Link != null)
				Output.Append("</a>");
		}
	
	}
}
