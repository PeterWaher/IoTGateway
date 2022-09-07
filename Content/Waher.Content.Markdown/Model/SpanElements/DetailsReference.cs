using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Details reference
	/// </summary>
	public class DetailsReference : MetaReference
	{
		/// <summary>
		/// Meta-data reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public DetailsReference(MarkdownDocument Document, string Key)
			: base(Document, Key)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			if (!(this.Document.Detail is null))
				await this.Document.Detail.GenerateHTML(Output, true);
			else
				await base.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("[%Details]");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			if (!(this.Document.Detail is null))
				await this.Document.Detail.GeneratePlainText(Output);
			else
				await base.GeneratePlainText(Output);
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			if (!(this.Document.Detail is null))
				await this.Document.Detail.GenerateXAML(Output, false);
			else
				await base.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			if (!(this.Document.Detail is null))
				await this.Document.Detail.GenerateXamarinForms(Output, false);
			else
				await base.GenerateXamarinForms(Output, State);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph
		{
			get { return true; }
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrDetailsReference++;
		}
	}
}
