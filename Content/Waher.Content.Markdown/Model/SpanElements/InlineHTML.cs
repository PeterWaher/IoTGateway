using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline HTML.
	/// </summary>
	public class InlineHTML : MarkdownElement
	{
		private string html;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="HTML">Inline HTML.</param>
		public InlineHTML(MarkdownDocument Document, string HTML)
			: base(Document)
		{
			this.html = HTML;
		}

		/// <summary>
		/// HTML
		/// </summary>
		public string HTML
		{
			get { return this.html; }
			internal set { this.html = value; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append(this.html);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.html;
		}
	}
}
