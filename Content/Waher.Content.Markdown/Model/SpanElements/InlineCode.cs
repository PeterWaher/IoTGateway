using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineCode : MarkdownElement
	{
		private string code;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Code">Inline Code.</param>
		public InlineCode(MarkdownDocument Document, string Code)
			: base(Document)
		{
			if (Code.StartsWith(" "))
				Code = Code.Substring(1);

			if (Code.EndsWith(" "))
				Code = Code.Substring(0, Code.Length - 1);

			this.code = Code;
		}

		/// <summary>
		/// Inline code.
		/// </summary>
		public string Code
		{
			get { return this.code; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<code>");
			Output.Append(MarkdownDocument.HtmlValueEncode(this.code));
			Output.Append("</code>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.code);
		}

	}
}
