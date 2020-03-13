using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Automatic Link (e-Mail)
	/// </summary>
	public class AutomaticLinkMail : MarkdownElement
	{
		private readonly string eMail;

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
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('<');
			Output.Append(this.eMail);
			Output.Append('>');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			string s = this.eMail;
			byte[] Data = System.Text.Encoding.ASCII.GetBytes(s);
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Data)
			{
				sb.Append("&#x");
				sb.Append(b.ToString("X2"));
				sb.Append(';');
			}

			s = sb.ToString();

			sb.Clear();
			Data = System.Text.Encoding.ASCII.GetBytes("mailto:");
			foreach (byte b in Data)
			{
				sb.Append("&#x");
				sb.Append(b.ToString("X2"));
				sb.Append(';');
			}

			Output.Append("<a href=\"");
			Output.Append(sb.ToString());
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

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("Hyperlink");
			Output.WriteAttributeString("NavigateUri", "mailto:" + this.eMail);
			Output.WriteValue(this.eMail);
			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("AutomaticLinkMail");
			Output.WriteAttributeString("eMail", this.eMail);
			Output.WriteEndElement();
		}
	}
}
