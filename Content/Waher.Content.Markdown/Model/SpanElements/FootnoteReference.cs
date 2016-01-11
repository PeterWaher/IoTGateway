using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Footnote reference
	/// </summary>
	public class FootnoteReference : MarkdownElement
	{
		private string key;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public FootnoteReference(MarkdownDocument Document, string Key)
			: base(Document)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			string s;
			int Nr;

			if (this.Document.TryGetFootnoteNumber(this.key, out Nr))
			{
				s = Nr.ToString();

				Output.Append("<sup id=\"fnref-");
				Output.Append(s);
				Output.Append("\"><a href=\"#fn-");
				Output.Append(s);
				Output.Append("\" class=\"footnote-ref\">");
				Output.Append(s);
				Output.Append("</a></sup>");
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			int Nr;

			if (this.Document.TryGetFootnoteNumber(this.key, out Nr))
			{
				Output.Append(" [");
				Output.Append(Nr.ToString());
				Output.Append("]");
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.key;
		}
	}
}
