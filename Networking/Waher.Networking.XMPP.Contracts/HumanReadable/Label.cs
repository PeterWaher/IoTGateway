using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Label
	/// </summary>
	public class Label : Formatting
	{
		private string language;

		/// <summary>
		/// Language of label
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
			Xml.Append("<label");
		
			if(!string.IsNullOrEmpty(this.language))
			{
				Xml.Append(" xml:lang=\"");
				Xml.Append(XML.Encode(this.language));
				Xml.Append('"');
			}

			Xml.Append('>');

			if (!(this.Elements is null))
			{
				foreach (HumanReadableElement E in this.Elements)
					E.Serialize(Xml);
			}
		
			Xml.Append("</label>");
		}

		/// <summary>
		/// Class representing human-readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public static Label Parse(System.Xml.XmlElement Xml)
		{
			Label Result = new Label()
			{
				language = XML.Attribute(Xml, "xml:lang"),
				Elements = InlineElement.ParseChildren(Xml)
			};

			return Result;
		}

		/// <summary>
		/// Parses simplified human readable text.
		/// </summary>
		/// <param name="Xml">XML representation.</param>
		/// <returns>Human-readable text.</returns>
		public static Label ParseSimplified(System.Xml.XmlElement Xml)
		{
			Label Result = new Label()
			{
				language = XML.Attribute(Xml, "lang"),
				Elements = new InlineElement[]
				{
					new Text()
					{
						Value = Xml.InnerText
					}
				}
			};

			return Result;
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
		/// Generates plain text for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Plain text</returns>
		public async Task<string> GeneratePlainText(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			return await Doc.GeneratePlainText();
		}

		/// <summary>
		/// Generates HTML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>HTML</returns>
		public async Task<string> GenerateHTML(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			return await Doc.GenerateHTML();
		}

		/// <summary>
		/// Generates WPF XAML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>WPF XAML</returns>
		public async Task<string> GenerateXAML(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			return await Doc.GenerateXAML();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the human-readable text.
		/// </summary>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <returns>Xamarin.Forms XAML</returns>
		public async Task<string> GenerateXamarinForms(Contract Contract)
		{
			string Markdown = this.GenerateMarkdown(Contract);
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			return await Doc.GenerateXamarinForms();
		}

	}
}
