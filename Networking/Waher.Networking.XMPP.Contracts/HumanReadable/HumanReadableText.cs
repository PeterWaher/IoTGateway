using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

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
		public static HumanReadableText Parse(XmlElement Xml)
		{
			HumanReadableText Result = new HumanReadableText()
			{
				language = XML.Attribute(Xml, "xml:lang"),
				Body = ParseChildren(Xml)
			};

			return Result;
		}

		/// <summary>
		/// Parses simplified human readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public static HumanReadableText ParseSimplified(XmlElement Xml)
		{
			HumanReadableText Result = new HumanReadableText()
			{
				language = XML.Attribute(Xml, "lang"),
				Body = new BlockElement[]
				{
					new Paragraph()
					{
						Elements = new InlineElement[]
						{
							new Text()
							{
								Value = Xml.InnerText
							}
						}
					}
				}
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
			this.Serialize(Xml, "humanReadableText", null);
		}

		/// <summary>
		/// Serializes the human-readable text, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="TagName">Local name of the text.</param>
		/// <param name="Namespace">Optional namespace to include. Can be null or empty.</param>
		public void Serialize(StringBuilder Xml, string TagName, string Namespace)
		{
			Xml.Append('<');
			Xml.Append(TagName);

			if (!string.IsNullOrEmpty(this.language))
			{
				Xml.Append(" xml:lang=\"");
				Xml.Append(XML.Encode(this.language));
				Xml.Append('"');
			}

			if (!string.IsNullOrEmpty(Namespace))
			{
				Xml.Append(" xmlns=\"");
				Xml.Append(Namespace);
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
			return this.GenerateMarkdown(Contract, MarkdownType.ForRendering);
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <param name="Type">Type of Markdown to generate</param>
		/// <returns>Markdown</returns>
		public string GenerateMarkdown(Contract Contract, MarkdownType Type)
		{
			return this.GenerateMarkdown(new MarkdownSettings(Contract, Type));
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		/// <returns>Markdown</returns>
		public string GenerateMarkdown(MarkdownSettings Settings)
		{
			MarkdownOutput Markdown = new MarkdownOutput();
			this.GenerateMarkdown(Markdown, 1, 0, Settings);
			return Markdown.ToString();
		}

		/// <summary>
		/// Generates a parsed markdown document for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Parsed Markdown</returns>
		public async Task<MarkdownDocument> GenerateMarkdownDocument(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			return await MarkdownDocument.CreateAsync(Markdown);
		}

		/// <summary>
		/// Generates plain text for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Plain text</returns>
		public async Task<string> GeneratePlainText(Contract Contract)
		{
			MarkdownDocument Doc = await this.GenerateMarkdownDocument(Contract);
			return await Doc.GeneratePlainText();
		}

		/// <summary>
		/// Generates HTML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>HTML</returns>
		public async Task<string> GenerateHTML(Contract Contract)
		{
			MarkdownDocument Doc = await this.GenerateMarkdownDocument(Contract);
			return await Doc.GenerateHTML();
		}
	}
}
