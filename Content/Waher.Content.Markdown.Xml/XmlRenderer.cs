using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Xml
{
	/// <summary>
	/// Renders XAML (WPF flavour) from a Markdown document.
	/// </summary>
	public class XmlRenderer : Renderer
	{
		/// <summary>
		/// XML output
		/// </summary>
		protected readonly XmlWriter xmlOutput;

		private bool elementsOpen = false;

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		public XmlRenderer()
			: this(XML.WriterSettings(false, true))
		{
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		public XmlRenderer(XmlWriterSettings XmlSettings)
			: base()
		{
			this.xmlOutput = XmlWriter.Create(this.Output, XmlSettings);
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="Output">Markdown output.</param>
		/// <param name="XmlSettings">XML-specific settings.</param>
		public XmlRenderer(StringBuilder Output, XmlWriterSettings XmlSettings)
			: base(Output)
		{
			this.xmlOutput = XmlWriter.Create(this.Output, XmlSettings);
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			this.xmlOutput.Dispose();
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			this.xmlOutput.WriteStartElement("parsedMakdown", "http://waher.se/Schema/Markdown.xsd");
			this.xmlOutput.WriteAttributeString("isDynamic", CommonTypes.Encode(this.Document.IsDynamic));

			if (!(this.Document.MetaData is null))
			{
				this.xmlOutput.WriteStartElement("metaData");

				foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> P in this.Document.MetaData)
				{
					this.xmlOutput.WriteStartElement("tag", P.Key);

					foreach (KeyValuePair<string, bool> P2 in P.Value)
					{
						this.xmlOutput.WriteStartElement("value", P2.Key);
						this.xmlOutput.WriteAttributeString("lineBreak", CommonTypes.Encode(P2.Value));
						this.xmlOutput.WriteEndElement();
					}

					this.xmlOutput.WriteEndElement();
				}

				this.xmlOutput.WriteEndElement();
			}

			this.xmlOutput.WriteStartElement("elements");
			this.elementsOpen = true;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			if (this.elementsOpen)
			{
				this.xmlOutput.WriteEndElement();
				this.elementsOpen = false;
			}

			await this.RenderReferences();

			if (!(this.Document.FootnoteOrder is null))
			{
				this.xmlOutput.WriteStartElement("footnotes");

				foreach (string s in this.Document.FootnoteOrder)
				{
					if ((this.Document?.TryGetFootnote(s, out Footnote F) ?? false) &&
						F.Referenced)
					{
						await F.Render(this);
					}
				}

				this.xmlOutput.WriteEndElement();
			}
		}

		private async Task RenderReferences()
		{
			if (!(this.Document.References is null))
			{
				this.xmlOutput.WriteStartElement("references");

				foreach (KeyValuePair<string, Multimedia> P in this.Document.References)
				{
					this.xmlOutput.WriteStartElement("reference");
					this.xmlOutput.WriteAttributeString("key", P.Key);

					await P.Value.Render(this);

					this.xmlOutput.WriteEndElement();
				}

				this.xmlOutput.WriteEndElement();
			}
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override async Task RenderDocumentFooter()
		{
			if (this.elementsOpen)
			{
				this.xmlOutput.WriteEndElement();
				this.elementsOpen = false;

				await this.RenderReferences();
			}

			this.xmlOutput.WriteEndElement();
			this.xmlOutput.Flush();
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Element">Element to export</param>
		/// <param name="ElementName">Name of element.</param>
		private async Task RenderElement(MarkdownElementChildren Element, string ElementName)
		{
			this.xmlOutput.WriteStartElement(ElementName);
			await this.RenderChildren(Element);
			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Element">Element to export</param>
		/// <param name="ElementName">Name of element.</param>
		private async Task RenderElement(MarkdownElementSingleChild Element, string ElementName)
		{
			this.xmlOutput.WriteStartElement(ElementName);
			await this.RenderChild(Element);
			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Element">Element to export</param>
		/// <param name="ElementName">Name of element.</param>
		private void RenderElement(MarkdownElement _, string ElementName)
		{
			this.xmlOutput.WriteElementString(ElementName, string.Empty);
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Abbreviation Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Abbreviation));
			this.xmlOutput.WriteAttributeString("description", Element.Description);

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkMail Element)
		{
			this.xmlOutput.WriteStartElement(nameof(AutomaticLinkMail));
			this.xmlOutput.WriteAttributeString("eMail", Element.EMail);
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.xmlOutput.WriteStartElement(nameof(AutomaticLinkUrl));
			this.xmlOutput.WriteAttributeString("url", Element.URL);
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Delete Element)
		{
			return this.RenderElement(Element, nameof(Delete));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DetailsReference Element)
		{
			this.RenderElement(Element, "Details");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(EmojiReference Element)
		{
			this.xmlOutput.WriteStartElement("Emoji");
			this.xmlOutput.WriteAttributeString("shortName", Element.Emoji.ShortName);

			if (Element.Level > 1)
				this.xmlOutput.WriteAttributeString("level", Element.Level.ToString());

			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Emphasize Element)
		{
			return this.RenderElement(Element, nameof(Emphasize));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			this.xmlOutput.WriteStartElement(nameof(FootnoteReference));
			this.xmlOutput.WriteAttributeString("key", Element.Key);

			if (this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false)
				this.xmlOutput.WriteAttributeString("nr", Nr.ToString());

			if (this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false)
			{
				if (Element.AutoExpand)
					await this.Render(Footnote);
				else
					Footnote.Referenced = true;
			}

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
			this.xmlOutput.WriteElementString(nameof(HashTag), Element.Tag);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
		{
			this.xmlOutput.WriteElementString(nameof(HtmlEntity), Element.Entity);

			return Task.CompletedTask; // TODO
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.xmlOutput.WriteElementString(nameof(HtmlEntityUnicode), Element.Code.ToString());

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.xmlOutput.WriteStartElement(nameof(InlineCode));
			this.xmlOutput.WriteCData(Element.Code);
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			this.xmlOutput.WriteStartElement("Html");
			this.xmlOutput.WriteCData(Element.HTML);
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineScript Element)
		{
			this.xmlOutput.WriteStartElement("Script");
			this.xmlOutput.WriteAttributeString("expression", Element.Expression.Script);
			this.xmlOutput.WriteAttributeString("aloneInParagraph", CommonTypes.Encode(Element.AloneInParagraph));
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.xmlOutput.WriteElementString(nameof(InlineText), Element.Value);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Insert Element)
		{
			return this.RenderElement(Element, nameof(Insert));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.xmlOutput.WriteElementString(nameof(LineBreak), string.Empty);

			return Task.CompletedTask; // TODO
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Link Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Link));
			this.xmlOutput.WriteAttributeString("url", Element.Url);
			this.xmlOutput.WriteAttributeString("title", Element.Title);

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(LinkReference Element)
		{
			this.xmlOutput.WriteStartElement(nameof(LinkReference));
			this.xmlOutput.WriteAttributeString("label", Element.Label);

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MetaReference Element)
		{
			this.xmlOutput.WriteStartElement(nameof(MetaReference));
			this.xmlOutput.WriteAttributeString("key", Element.Key);
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Multimedia Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Multimedia));
			this.xmlOutput.WriteAttributeString("aloneInParagraph", CommonTypes.Encode(Element.AloneInParagraph));

			await this.RenderChildren(Element);

			foreach (MultimediaItem Item in Element.Items)
				Item.Export(this.xmlOutput);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MultimediaReference Element)
		{
			this.xmlOutput.WriteStartElement(nameof(MultimediaReference));
			this.xmlOutput.WriteAttributeString("label", Element.Label);

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(StrikeThrough Element)
		{
			return this.RenderElement(Element, nameof(StrikeThrough));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Strong Element)
		{
			return this.RenderElement(Element, nameof(Strong));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SubScript Element)
		{
			return this.RenderElement(Element, nameof(SubScript));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SuperScript Element)
		{
			return this.RenderElement(Element, nameof(SuperScript));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Underline Element)
		{
			return this.RenderElement(Element, nameof(Underline));
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(BlockQuote Element)
		{
			return this.RenderElement(Element, nameof(BlockQuote));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(BulletList Element)
		{
			return this.RenderElement(Element, nameof(BulletList));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CenterAligned Element)
		{
			return this.RenderElement(Element, nameof(CenterAligned));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CodeBlock Element)
		{
			this.xmlOutput.WriteStartElement(nameof(CodeBlock));
			this.xmlOutput.WriteAttributeString("language", Element.Language);
			this.xmlOutput.WriteAttributeString("start", Element.Start.ToString());
			this.xmlOutput.WriteAttributeString("end", Element.End.ToString());
			this.xmlOutput.WriteAttributeString("indent", Element.Indent.ToString());
			this.xmlOutput.WriteAttributeString("indentString", Element.IndentString);

			int i;

			for (i = Element.Start; i <= Element.End; i++)
				this.xmlOutput.WriteElementString("Row", Element.Rows[i]);

			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CommentBlock Element)
		{
			this.xmlOutput.WriteStartElement(nameof(CommentBlock));

			foreach (string s in Element.Rows)
				this.xmlOutput.WriteElementString("Row", s);

			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionDescriptions Element)
		{
			return this.RenderElement(Element, nameof(DefinitionDescriptions));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionList Element)
		{
			return this.RenderElement(Element, nameof(DefinitionList));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionTerms Element)
		{
			return this.RenderElement(Element, nameof(DefinitionTerms));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DeleteBlocks Element)
		{
			return this.RenderElement(Element, nameof(DeleteBlocks));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Footnote Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Footnote));
			this.xmlOutput.WriteAttributeString("key", Element.Key);

			if (Element.Document.TryGetFootnoteNumber(Element.Key, out int Nr))
				this.xmlOutput.WriteAttributeString("nr", Nr.ToString());

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Header Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Header));
			this.xmlOutput.WriteAttributeString("id", await Element.Id);
			this.xmlOutput.WriteAttributeString("level", Element.Level.ToString());

			await this.RenderChildren(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.RenderElement(Element, nameof(HorizontalRule));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlBlock Element)
		{
			return this.RenderElement(Element, nameof(HtmlBlock));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InsertBlocks Element)
		{
			return this.RenderElement(Element, nameof(InsertBlocks));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InvisibleBreak Element)
		{
			this.RenderElement(Element, nameof(InvisibleBreak));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LeftAligned Element)
		{
			return this.RenderElement(Element, nameof(LeftAligned));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MarginAligned Element)
		{
			return this.RenderElement(Element, nameof(MarginAligned));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(NestedBlock Element)
		{
			return this.RenderElement(Element, nameof(NestedBlock));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedItem Element)
		{
			this.xmlOutput.WriteStartElement(nameof(NumberedItem));
			this.xmlOutput.WriteAttributeString("number", Element.Number.ToString());
			this.xmlOutput.WriteAttributeString("explicit", CommonTypes.Encode(Element.NumberExplicit));

			await this.RenderChild(Element);

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(NumberedList Element)
		{
			return this.RenderElement(Element, nameof(NumberedList));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Paragraph));

			if (Element.Implicit)
				this.xmlOutput.WriteAttributeString("implicit", CommonTypes.Encode(Element.Implicit));

			await this.RenderChildren(Element);
			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(RightAligned Element)
		{
			return this.RenderElement(Element, nameof(RightAligned));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Sections Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Sections));
			this.xmlOutput.WriteAttributeString("nrColumns", Element.InitialNrColumns.ToString());
			await this.RenderChildren(Element);
			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.xmlOutput.WriteStartElement(nameof(SectionSeparator));
			this.xmlOutput.WriteAttributeString("sectionNr", Element.SectionNr.ToString());
			this.xmlOutput.WriteAttributeString("nrColumns", Element.NrColumns.ToString());
			this.xmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			this.xmlOutput.WriteStartElement(nameof(Table));
			this.xmlOutput.WriteAttributeString("caption", Element.Caption);
			this.xmlOutput.WriteAttributeString("id", Element.Id);
			this.xmlOutput.WriteAttributeString("columns", Element.Columns.ToString());

			foreach (TextAlignment Col in Element.Alignments)
			{
				this.xmlOutput.WriteStartElement("Column");
				this.xmlOutput.WriteAttributeString("alignment", Col.ToString());
				this.xmlOutput.WriteEndElement();
			}

			foreach (MarkdownElement[] Row in Element.Headers)
			{
				this.xmlOutput.WriteStartElement("HeaderRow");

				foreach (MarkdownElement E in Row)
				{
					if (E is null)
						this.xmlOutput.WriteElementString("Continue", string.Empty);
					else
					{
						this.xmlOutput.WriteStartElement("HeaderCell");
						await E.Render(this);
						this.xmlOutput.WriteEndElement();
					}
				}

				this.xmlOutput.WriteEndElement();
			}

			foreach (MarkdownElement[] Row in Element.Rows)
			{
				this.xmlOutput.WriteStartElement("Row");

				foreach (MarkdownElement E in Row)
				{
					if (E is null)
						this.xmlOutput.WriteElementString("Continue", string.Empty);
					else
					{
						this.xmlOutput.WriteStartElement("Cell");
						await E.Render(this);
						this.xmlOutput.WriteEndElement();
					}
				}

				this.xmlOutput.WriteEndElement();
			}

			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.xmlOutput.WriteStartElement(nameof(TaskItem));
			this.xmlOutput.WriteAttributeString("isChecked", CommonTypes.Encode(Element.IsChecked));
			await this.RenderChild(Element);
			this.xmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(TaskList Element)
		{
			return this.RenderElement(Element, nameof(TaskList));
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.xmlOutput.WriteStartElement("UnnumberedItem");
			this.xmlOutput.WriteAttributeString("prefix", Element.Prefix);
			await this.RenderChild(Element);
			this.xmlOutput.WriteEndElement();
		}

		#endregion

	}
}
