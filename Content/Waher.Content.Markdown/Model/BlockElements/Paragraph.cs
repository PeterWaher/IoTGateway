using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a paragraph in a markdown document.
	/// </summary>
	public class Paragraph : BlockElementChildren
	{
		private readonly bool @implicit;

		/// <summary>
		/// Represents a paragraph in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public Paragraph(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.@implicit = false;
		}

		/// <summary>
		/// Represents a paragraph in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		/// <param name="Implicit">If paragraph is implicit or not.</param>
		public Paragraph(MarkdownDocument Document, IEnumerable<MarkdownElement> Children,
			bool Implicit)
			: base(Document, Children)
		{
			this.@implicit = Implicit;
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			await base.GenerateMarkdown(Output);
			Output.AppendLine();
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			if (!this.@implicit)
				Output.Append("<p>");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);

			if (!this.@implicit)
				Output.AppendLine("</p>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GeneratePlainText(Output);

			Output.AppendLine();
			Output.AppendLine();
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;
			string s;

			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("TextWrapping", "Wrap");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			foreach (MarkdownElement E in this.Children)
			{
				if ((!E.InlineSpanElement || E.OutsideParagraph) && (s = E.BaselineAlignment) != "Baseline")
				{
					Output.WriteStartElement("InlineUIContainer");
					Output.WriteAttributeString("BaselineAlignment", s);

					await E.GenerateXAML(Output, TextAlignment);

					Output.WriteEndElement();
				}
				else
					await E.GenerateXAML(Output, TextAlignment);
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			GenerateXamarinFormsContentView(Output, State.TextAlignment, this.Document.Settings.XamlSettings);
			await GenerateXamarinFormsLabel(Output, this, false, State);
			Output.WriteEndElement();
		}

		internal static async Task GenerateXamarinFormsLabel(XmlWriter Output, MarkdownElement Element, bool IncludeElement, XamarinRenderingState State)
		{
			bool HasLink = !Element.ForEach((E, _) =>
			{
				return !(
					E is AutomaticLinkMail ||
					E is AutomaticLinkUrl ||
					E is Link ||
					E is LinkReference);
			}, null);

			Output.WriteStartElement("Label");
			Output.WriteAttributeString("LineBreakMode", "WordWrap");
			Header.XamarinFormsLabelAlignment(Output, State);

			if (HasLink)
			{
				if (State.InLabel)
				{
					if (IncludeElement)
						await Element.GenerateXamarinForms(Output, State);
					else
					{
						foreach (MarkdownElement E in Element.Children)
							await E.GenerateXamarinForms(Output, State);
					}
				}
				else
				{
					State.InLabel = true;

					Output.WriteStartElement("Label.FormattedText");
					Output.WriteStartElement("FormattedString");

					if (IncludeElement)
						await Element.GenerateXamarinForms(Output, State);
					else
					{
						foreach (MarkdownElement E in Element.Children)
							await E.GenerateXamarinForms(Output, State);
					}

					Output.WriteEndElement();
					Output.WriteEndElement();

					State.InLabel = false;
				}
			}
			else
			{
				Output.WriteAttributeString("TextType", "Html");

				if (State.Bold)
					Output.WriteAttributeString("FontAttributes", "Bold");

				StringBuilder Html = new StringBuilder();

				if (IncludeElement)
					await Element.GenerateHTML(Html);
				else
				{
					foreach (MarkdownElement E in Element.Children)
						await E.GenerateHTML(Html);
				}

				Output.WriteCData(Html.ToString());
			}

			Output.WriteEndElement();
		}

		internal static void GenerateXamarinFormsSpan(XmlWriter Output, string Text, XamarinRenderingState State)
		{
			if (!State.InLabel)
			{
				Output.WriteStartElement("Label");
				Output.WriteAttributeString("LineBreakMode", "WordWrap");
				Header.XamarinFormsLabelAlignment(Output, State);
				Output.WriteStartElement("Label.FormattedText");
				Output.WriteStartElement("FormattedString");
			}

			Output.WriteStartElement("Span");

			if (State.Superscript)
				Text = SuperScript.ToSuperscript(Text);
			else if (State.Subscript)
				Text = SubScript.ToSubscript(Text);

			Output.WriteAttributeString("Text", Text);

			if (State.Bold && State.Italic)
				Output.WriteAttributeString("FontAttributes", "Italic, Bold");
			else if (State.Bold)
				Output.WriteAttributeString("FontAttributes", "Bold");
			else if (State.Italic)
				Output.WriteAttributeString("FontAttributes", "Italic");

			if (State.StrikeThrough && State.Underline)
				Output.WriteAttributeString("TextDecorations", "Strikethrough, Underline");
			else if (State.StrikeThrough)
				Output.WriteAttributeString("TextDecorations", "Strikethrough");
			else if (State.Underline)
				Output.WriteAttributeString("TextDecorations", "Underline");

			if (State.Code)
				Output.WriteAttributeString("FontFamily", "Courier New");

			if (!(State.Hyperlink is null))
			{
				Output.WriteAttributeString("TextColor", "{Binding HyperlinkColor}");

				Output.WriteStartElement("Span.GestureRecognizers");
				Output.WriteStartElement("TapGestureRecognizer");
				Output.WriteAttributeString("Command", "{Binding HyperlinkClicked}");
				Output.WriteAttributeString("CommandParameter", State.Hyperlink);
				Output.WriteEndElement();
				Output.WriteEndElement();
			}

			if (!State.InLabel)
			{
				Output.WriteEndElement();
				Output.WriteEndElement();
				Output.WriteEndElement();
			}

			Output.WriteEndElement();
		}

		internal static void GenerateXamarinFormsContentView(XmlWriter Output, TextAlignment TextAlignment, XamlSettings Settings)
		{
			GenerateXamarinFormsContentView(Output, TextAlignment, Settings.ParagraphMargins);
		}

		internal static void GenerateXamarinFormsContentView(XmlWriter Output, TextAlignment TextAlignment, string Margins)
		{
			Output.WriteStartElement("ContentView");
			Output.WriteAttributeString("Padding", Margins);

			switch (TextAlignment)
			{
				case TextAlignment.Center:
					Output.WriteAttributeString("HorizontalOptions", "Center");
					break;

				case TextAlignment.Left:
					Output.WriteAttributeString("HorizontalOptions", "Start");
					break;

				case TextAlignment.Right:
					Output.WriteAttributeString("HorizontalOptions", "End");
					break;
			}
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			Output.WriteStartElement("paragraph");
			await base.GenerateSmartContractXml(Output, State);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateLaTeX(Output);

			Output.AppendLine();
			Output.AppendLine();
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
			Output.WriteStartElement("Paragraph");

			if (this.@implicit)
				Output.WriteAttributeString("implicit", CommonTypes.Encode(this.@implicit));

			this.ExportChildren(Output);
			Output.WriteEndElement();
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
			return new Paragraph(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrParagraph++;
		}

	}
}
