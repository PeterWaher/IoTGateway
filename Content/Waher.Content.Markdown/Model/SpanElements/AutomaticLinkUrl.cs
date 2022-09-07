using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Automatic Link (URL)
	/// </summary>
	public class AutomaticLinkUrl : MarkdownElement
	{
		private readonly string url;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="URL">Automatic URL link.</param>
		public AutomaticLinkUrl(MarkdownDocument Document, string URL)
			: base(Document)
		{
			this.url = URL;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string URL => this.url;

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('<');
			Output.Append(this.url);
			Output.Append('>');
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override Task GenerateHTML(StringBuilder Output)
		{
			bool IsRelative = this.url.IndexOf(':') < 0;

			Output.Append("<a href=\"");
			Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(this.url, null)));

			if (!IsRelative)
				Output.Append("\" target=\"_blank");

			Output.Append("\">");
			Output.Append(XML.HtmlValueEncode(this.url));
			Output.Append("</a>");
	
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override Task GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.url);
	
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.url;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Output.WriteStartElement("Hyperlink");
			Output.WriteAttributeString("NavigateUri", Document.CheckURL(this.url, null));
			Output.WriteValue(this.url);
			Output.WriteEndElement();			
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			string Bak = State.Hyperlink;
			State.Hyperlink = this.url;
			Paragraph.GenerateXamarinFormsSpan(Output, this.url, State);
			State.Hyperlink = Bak;

			return Task.CompletedTask;
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => true;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("AutomaticLinkUrl");
			Output.WriteAttributeString("url", this.url);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is AutomaticLinkUrl x &&
				this.url == x.url &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.url?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			Output.WriteElementString("text", this.url);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrUrlHyperLinks++;
			Statistics.NrHyperLinks++;
		}

	}
}
