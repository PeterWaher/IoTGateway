using System;
using System.Collections.Generic;
using System.Text;
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
		public override void GenerateMarkdown(StringBuilder Output)
		{
			PrefixedBlock(Output, this.Children, ":\t", "\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				Output.Append("<dd>");
				E.GenerateHTML(Output);
				Output.AppendLine("</dd>");
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
			{
				Output.Append(":\t");
				E.GeneratePlainText(Output);
				Output.AppendLine();
			}
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
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

				Description.GenerateXAML(Output, TextAlignment);
				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			MarkdownElement Last = null;

			foreach (MarkdownElement Description in this.Children)
				Last = Description;

			foreach (MarkdownElement Description in this.Children)
			{
				if (Description.InlineSpanElement && !Description.OutsideParagraph)
				{
					Paragraph.GenerateXamarinFormsContentView(Output, TextAlignment, Settings);

					Output.WriteStartElement("Label");
					Output.WriteAttributeString("LineBreakMode", "WordWrap");
					Output.WriteAttributeString("TextType", "Html");

					StringBuilder Html = new StringBuilder();
					Description.GenerateHTML(Html);
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
					Description.GenerateXamarinForms(Output, TextAlignment);
					Output.WriteEndElement();

					Output.WriteEndElement();
				}
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

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

	}
}
