using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link
	/// </summary>
	public class Link : MarkdownElementChildren
	{
		private string url;
		private string title;

		/// <summary>
		/// Link
		/// </summary>
		public Link(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Url, string Title)
			: base(Document, ChildElements)
		{
			this.url = Url;
			this.title = Title;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string Url
		{
			get { return this.url; }
		}

		/// <summary>
		/// Optional Link title.
		/// </summary>
		public string Title
		{
			get { return this.title; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<a href=\"");
			Output.Append(MarkdownDocument.HtmlEncode(this.url));

			if (!string.IsNullOrEmpty(this.title))
			{
				Output.Append("\" title=\"");
				Output.Append(MarkdownDocument.HtmlEncode(this.title));
			}

			Output.Append("\">");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</a>");
		}
	
	}
}
