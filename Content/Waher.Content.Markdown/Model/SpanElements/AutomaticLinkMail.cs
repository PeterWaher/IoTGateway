using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Automatic Link (e-Mail)
	/// </summary>
	public class AutomaticLinkMail : MarkdownElement
	{
		private string eMail;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="EMail">Automatic e-Mail link.</param>
		public AutomaticLinkMail(MarkdownDocument Document, string EMail)
			: base(Document)
		{
			this.eMail = EMail;
		}

		/// <summary>
		/// e-Mail
		/// </summary>
		public string EMail
		{
			get { return this.eMail; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			string s = "mailto:" + this.eMail;
			byte[] Data = System.Text.Encoding.ASCII.GetBytes(s);
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Data)
			{
				sb.Append("&#x");
				sb.Append(b.ToString("X2"));
				sb.Append(';');
			}

			s = sb.ToString();

			Output.Append("<a href=\"");
			Output.Append(s);
			Output.Append("\">");
			Output.Append(s);
			Output.Append("</a>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.eMail);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.eMail;
		}
	}
}
