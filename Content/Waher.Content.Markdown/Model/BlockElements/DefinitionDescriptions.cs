using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition descriptions
	/// </summary>
	public class DefinitionDescriptions : BlockElementChildren
	{
		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, IEnumerable<MarkdownElement> Descriptions)
			: base(Document, Descriptions)
		{
		}

		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, params MarkdownElement[] Descriptions)
			: base(Document, Descriptions)
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			await PrefixedBlock(Output, this.Children, ":\t", "\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				Output.Append("<dd>");
				await E.GenerateHTML(Output);
				Output.AppendLine("</dd>");
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				Output.Append(":\t");
				await E.GeneratePlainText(Output);
				Output.AppendLine();
			}
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			MarkdownElement Last = null;

			foreach (MarkdownElement Description in this.Children)
				Last = Description;

			foreach (MarkdownElement Description in this.Children)
			{
				if (Description.InlineSpanElement && !Description.OutsideParagraph)
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
				}
				else
					Output.WriteStartElement("StackPanel");

				Output.WriteAttributeString("Margin", Settings.DefinitionMargin.ToString() + ",0,0," +
					(Description == Last ? Settings.DefinitionSeparator : 0).ToString());

				await Description.GenerateXAML(Output, TextAlignment);
				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			MarkdownElement Last = null;

			foreach (MarkdownElement Description in this.Children)
				Last = Description;

			foreach (MarkdownElement Description in this.Children)
			{
				if (Description.InlineSpanElement && !Description.OutsideParagraph)
				{
					Paragraph.GenerateXamarinFormsContentView(Output, State.TextAlignment, Settings);

					Output.WriteStartElement("Label");
					Output.WriteAttributeString("LineBreakMode", "WordWrap");
					Header.XamarinFormsLabelAlignment(Output, State);
					Output.WriteAttributeString("TextType", "Html");

					StringBuilder Html = new StringBuilder();
					await Description.GenerateHTML(Html);
					Output.WriteCData(Html.ToString());

					Output.WriteEndElement();
					Output.WriteEndElement();
				}
				else
				{
					Output.WriteStartElement("ContentView");
					Output.WriteAttributeString("Padding", Settings.DefinitionMargin.ToString() + ",0,0," +
						(Description == Last ? Settings.DefinitionSeparator : 0).ToString());

					Output.WriteStartElement("StackLayout");
					await Description.GenerateXamarinForms(Output, State);
					Output.WriteEndElement();

					Output.WriteEndElement();
				}
			}
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateLaTeX(Output);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => false;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "DefinitionDescription");
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new DefinitionDescriptions(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrDefinitionDescriptions++;
		}

	}
}
