using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition descriptions
	/// </summary>
	public class DefinitionDescriptions : MarkdownElementChildren
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
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
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

				Description.GenerateXAML(Output, Settings, TextAlignment);
				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

	}
}
