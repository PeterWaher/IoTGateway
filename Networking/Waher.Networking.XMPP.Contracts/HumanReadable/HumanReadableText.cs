using System.Text;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Class representing human-readable text.
	/// </summary>
	public class HumanReadableText : Blocks
	{
		private string language = string.Empty;

		/// <summary>
		/// Class representing human-readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public new static HumanReadableText Parse(XmlElement Xml)
		{
			HumanReadableText Result = new HumanReadableText()
			{
				language = XML.Attribute(Xml, "xml:lang"),
				Body = BlockElement.Parse(Xml)
			};

			return Result;
		}

		/// <summary>
		/// Optional language
		/// </summary>
		public string Language
		{
			get => this.language;
			set => this.language = value;
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			this.Serialize(Xml, "humanReadableText", false);
		}

		/// <summary>
		/// Serializes the human-readable text, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="TagName">Local name of the text.</param>
		/// <param name="IncludeNamespace">If namespace attribute should be included.</param>
		public void Serialize(StringBuilder Xml, string TagName, bool IncludeNamespace)
		{
			Xml.Append('<');
			Xml.Append(TagName);

			if (!string.IsNullOrEmpty(this.language))
			{
				Xml.Append(" xml=\"");
				Xml.Append(XML.Encode(this.language));
				Xml.Append('"');
			}

			if (IncludeNamespace)
			{
				Xml.Append(" xmlns=\"");
				Xml.Append(ContractsClient.NamespaceSmartContracts);
				Xml.Append('"');
			}

			if (this.Body is null || this.Body.Length == 0)
				Xml.Append("/>");
			else
			{
				Xml.Append('>');

				foreach (BlockElement Block in this.Body)
					Block.Serialize(Xml);

				Xml.Append("</");
				Xml.Append(TagName);
				Xml.Append('>');
			}
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Markdown</returns>
		public string GenerateMarkdown(Contract Contract)
		{
			StringBuilder Markdown = new StringBuilder();
			this.GenerateMarkdown(Markdown, 1, Contract);
			return Markdown.ToString();
		}

		/// <summary>
		/// Generates plain text for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Plain text</returns>
		public string GeneratePlainText(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = new MarkdownDocument(Markdown);
			return Doc.GeneratePlainText();
		}

		/// <summary>
		/// Generates HTML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>HTML</returns>
		public string GenerateHTML(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = new MarkdownDocument(Markdown);
			return Doc.GenerateHTML();
		}

		/// <summary>
		/// Generates XAML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>XAML</returns>
		public string GenerateXAML(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = new MarkdownDocument(Markdown);
			return Doc.GenerateXAML();
		}

	}
}
