using SkiaSharp;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Contracts
{
	/// <summary>
	/// Renders Contracts XML from a Markdown document.
	/// </summary>
	public class ContractsRenderer : Renderer
	{
		/// <summary>
		/// XML output
		/// </summary>
		public readonly XmlWriter XmlOutput;

		/// <summary>
		/// Local Name of container element, or null if none.
		/// </summary>
		public readonly string LocalName;

		/// <summary>
		/// Namespace of container element, or null if none specified.
		/// </summary>
		public readonly string Namespace;

		/// <summary>
		/// Current section header level.
		/// </summary>
		public int Level = 0;

		/// <summary>
		/// Renders Contracts XML from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="LocalName">Local Name of container element. If no container element, LocalName is null.</param>
		public ContractsRenderer(XmlWriterSettings XmlSettings, string LocalName)
			: this(XmlSettings, LocalName, null)
		{
		}

		/// <summary>
		/// Renders Contracts XML from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="LocalName">Local Name of container element. If no container element, LocalName is null.</param>
		/// <param name="Namespace">Namespace of container element, or null if none specified.</param>
		public ContractsRenderer(XmlWriterSettings XmlSettings, string LocalName, string Namespace)
			: base()
		{
			this.XmlOutput = XmlWriter.Create(this.Output, XmlSettings);
			this.LocalName = LocalName;
			this.Namespace = Namespace;
		}

		/// <summary>
		/// Renders Contracts XML from a Markdown document.
		/// </summary>
		/// <param name="Output">Contract XML output.</param>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="LocalName">Local Name of container element. If no container element, LocalName is null.</param>
		public ContractsRenderer(StringBuilder Output, XmlWriterSettings XmlSettings, string LocalName)
			: this(Output, XmlSettings, LocalName, null)
		{
		}

		/// <summary>
		/// Renders Contracts XML from a Markdown document.
		/// </summary>
		/// <param name="Output">Contract XML output.</param>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="LocalName">Local Name of container element. If no container element, LocalName is null.</param>
		/// <param name="Namespace">Namespace of container element, or null if none specified.</param>
		public ContractsRenderer(StringBuilder Output, XmlWriterSettings XmlSettings, string LocalName, string Namespace)
			: base(Output)
		{
			this.XmlOutput = XmlWriter.Create(this.Output, XmlSettings);
			this.LocalName = LocalName;
			this.Namespace = Namespace;
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			this.XmlOutput.Dispose();
		}

		/// <summary>
		/// Renders a document.
		/// </summary>
		/// <param name="Document">Document to render.</param>
		/// <param name="Inclusion">If the rendered output is to be included in another document (true), or if it is a standalone document (false).</param>
		public override async Task RenderDocument(MarkdownDocument Document, bool Inclusion)
		{
			this.Level = 0;

			await base.RenderDocument(Document, Inclusion);

			while (this.Level > 0)
			{
				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
				this.Level--;
			}

			this.XmlOutput.Flush();
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			if (!string.IsNullOrEmpty(this.LocalName))
			{
				if (string.IsNullOrEmpty(this.Namespace))
					this.XmlOutput.WriteStartElement(this.LocalName);
				else
					this.XmlOutput.WriteStartElement(this.LocalName, this.Namespace);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			int ExpectedNr = 1;
			bool FootNotesAdded = false;

			while (this.Level > 0)
			{
				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
				this.Level--;
			}

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if (!(this.Document?.TryGetFootnoteNumber(Key, out int Nr) ?? false) ||
					Nr != ExpectedNr ||
					!(this.Document?.TryGetFootnote(Key, out Footnote Note) ?? false) ||
					!Note.Referenced)
				{
					continue;
				}

				ExpectedNr++;

				if (!FootNotesAdded)
				{
					FootNotesAdded = true;
					this.XmlOutput.WriteElementString("separator", string.Empty);
					this.XmlOutput.WriteStartElement("numberedItems");
				}

				this.XmlOutput.WriteStartElement("item");
				await Note.Render(this);
				this.XmlOutput.WriteEndElement();
			}

			if (FootNotesAdded)
				this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
			if (!string.IsNullOrEmpty(this.LocalName))
				this.XmlOutput.WriteEndElement();
			
			this.XmlOutput.Flush();

			return Task.CompletedTask;
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Abbreviation Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkMail Element)
		{
			this.XmlOutput.WriteElementString("text", Element.EMail);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.XmlOutput.WriteElementString("text", Element.URL);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Delete Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DetailsReference Element)
		{
			return this.Render((MetaReference)Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(EmojiReference Element)
		{
			this.XmlOutput.WriteElementString("text", Element.Emoji.Unicode);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.XmlOutput.WriteStartElement("italic");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			if (!(this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false))
				Footnote = null;

			if (Element.AutoExpand && !(Footnote is null))
				await this.Render(Footnote);
			else if (this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false)
			{
				this.XmlOutput.WriteStartElement("super");
				this.XmlOutput.WriteElementString("text", Nr.ToString());
				this.XmlOutput.WriteEndElement();

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
			this.XmlOutput.WriteElementString("text", Element.Tag);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
		{
			string s = Html.HtmlEntity.EntityToCharacter(Element.Entity);
			if (!string.IsNullOrEmpty(s))
				this.XmlOutput.WriteElementString("text", s);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.XmlOutput.WriteElementString("text", new string((char)Element.Code, 1));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.XmlOutput.WriteElementString("text", Element.Code);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			return Task.CompletedTask; // TODO
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InlineScript Element)
		{
			object Result = await Element.EvaluateExpression();

			await this.RenderObject(Result, Element.AloneInParagraph, Element.Variables);
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		public async Task RenderObject(object Result, bool AloneInParagraph, Variables Variables)
		{
			if (Result is null)
				return;

			if (Result is XmlDocument Xml)
				Result = await MarkdownDocument.TransformXml(Xml, Variables);
			else if (Result is IToMatrix ToMatrix)
				Result = ToMatrix.ToMatrix();

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels(Variables, out GraphSettings GraphSettings);
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					this.XmlOutput.WriteStartElement("imageStandalone");
				else
					this.XmlOutput.WriteStartElement("imageInline");

				this.XmlOutput.WriteAttributeString("contentType", "image/png");
				this.XmlOutput.WriteAttributeString("width", GraphSettings.Width.ToString());
				this.XmlOutput.WriteAttributeString("height", GraphSettings.Height.ToString());

				this.XmlOutput.WriteStartElement("binary");
				this.XmlOutput.WriteValue(Convert.ToBase64String(Bin));
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteStartElement("caption");
				if (G is Graph2D Graph2D && !string.IsNullOrEmpty(Graph2D.Title))
					this.XmlOutput.WriteElementString("text", Graph2D.Title);
				else
					this.XmlOutput.WriteElementString("text", "Graph");

				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					this.XmlOutput.WriteStartElement("imageStandalone");
				else
					this.XmlOutput.WriteStartElement("imageInline");

				this.XmlOutput.WriteAttributeString("contentType", "image/png");
				this.XmlOutput.WriteAttributeString("width", Pixels.Width.ToString());
				this.XmlOutput.WriteAttributeString("height", Pixels.Height.ToString());

				this.XmlOutput.WriteStartElement("binary");
				this.XmlOutput.WriteValue(Convert.ToBase64String(Bin));
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteStartElement("caption");
				this.XmlOutput.WriteElementString("text", "Image");
				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					if (AloneInParagraph)
						this.XmlOutput.WriteStartElement("imageStandalone");
					else
						this.XmlOutput.WriteStartElement("imageInline");

					this.XmlOutput.WriteAttributeString("contentType", "image/png");
					this.XmlOutput.WriteAttributeString("width", Img.Width.ToString());
					this.XmlOutput.WriteAttributeString("height", Img.Height.ToString());

					this.XmlOutput.WriteStartElement("binary");
					this.XmlOutput.WriteValue(Convert.ToBase64String(Bin));
					this.XmlOutput.WriteEndElement();

					this.XmlOutput.WriteStartElement("caption");
					this.XmlOutput.WriteElementString("text", "Image");
					this.XmlOutput.WriteEndElement();
					this.XmlOutput.WriteEndElement();
				}
			}
			else if (Result is MarkdownDocument Doc)
			{
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is MarkdownContent Markdown)
			{
				Doc = await MarkdownDocument.CreateAsync(Markdown.Markdown);
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is Exception ex)
			{
				bool First = true;

				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						if (AloneInParagraph)
							this.XmlOutput.WriteStartElement("paragraph");

						foreach (string Row in ex3.Message.Replace("\r\n", "\n").
							Replace('\r', '\n').Split('\n'))
						{
							if (First)
								First = false;
							else
								this.XmlOutput.WriteElementString("lineBreak", string.Empty);

							this.XmlOutput.WriteElementString("text", Result?.ToString() ?? string.Empty);
						}

						if (AloneInParagraph)
							this.XmlOutput.WriteEndElement();
					}
				}
				else
				{
					if (AloneInParagraph)
						this.XmlOutput.WriteStartElement("paragraph");

					foreach (string Row in ex.Message.Replace("\r\n", "\n").
						Replace('\r', '\n').Split('\n'))
					{
						if (First)
							First = false;
						else
							this.XmlOutput.WriteElementString("lineBreak", string.Empty);

						this.XmlOutput.WriteElementString("text", Result?.ToString() ?? string.Empty);
					}

					if (AloneInParagraph)
						this.XmlOutput.WriteEndElement();
				}
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await this.RenderObject(Item, false, Variables);
			}
			else
			{
				if (AloneInParagraph)
					this.XmlOutput.WriteStartElement("paragraph");

				this.XmlOutput.WriteElementString("text", Result?.ToString() ?? string.Empty);

				if (AloneInParagraph)
					this.XmlOutput.WriteEndElement();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.XmlOutput.WriteElementString("text", Element.Value);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Insert Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.XmlOutput.WriteElementString("lineBreak", string.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Link Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LinkReference Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MetaReference Element)
		{
			this.XmlOutput.WriteStartElement("parameter");
			this.XmlOutput.WriteAttributeString("name", Element.Key);
			this.XmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Model.SpanElements.Multimedia Element)
		{
			IMultimediaContractsRenderer Renderer = Element.MultimediaHandler<IMultimediaContractsRenderer>();
			if (Renderer is null)
				return this.RenderChildren(Element);
			else
				return Renderer.RenderContractXml(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MultimediaReference Element)
		{
			Model.SpanElements.Multimedia Multimedia = Element.Document.GetReference(Element.Label);

			if (!(Multimedia is null))
			{
				IMultimediaContractsRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaContractsRenderer>();
				if (!(Renderer is null))
					return Renderer.RenderContractXml(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.XmlOutput.WriteStartElement("strikeThrough");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.XmlOutput.WriteStartElement("bold");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.XmlOutput.WriteStartElement("sub");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.XmlOutput.WriteStartElement("super");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.XmlOutput.WriteStartElement("underline");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(BlockQuote Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			this.XmlOutput.WriteStartElement("bulletItems");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CenterAligned Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			ICodeContentContractsRenderer Renderer = Element.CodeContentHandler<ICodeContentContractsRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					if (await Renderer.RenderContractXml(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception)
				{
					// Continue
				}
			}

			this.XmlOutput.WriteStartElement("paragraph");

			int i;
			bool First = true;

			for (i = Element.Start; i <= Element.End; i++)
			{
				if (First)
					First = false;
				else
					this.XmlOutput.WriteElementString("lineBreak", string.Empty);

				this.XmlOutput.WriteElementString("text", Element.Rows[i]);
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CommentBlock Element)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionDescriptions Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionList Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionTerms Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DeleteBlocks Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Footnote Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Header Element)
		{
			while (this.Level >= Element.Level)
			{
				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
				this.Level--;
			}

			this.XmlOutput.WriteStartElement("section");
			this.XmlOutput.WriteStartElement("header");

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("body");

			this.Level++;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.XmlOutput.WriteElementString("separator", string.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			this.XmlOutput.WriteStartElement("paragraph");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InsertBlocks Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InvisibleBreak Element)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LeftAligned Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MarginAligned Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(NestedBlock Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedItem Element)
		{
			this.XmlOutput.WriteStartElement("item");

			if (Element.Child is Paragraph P)
				await this.RenderChildren(P);
			else
				await this.RenderChild(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			this.XmlOutput.WriteStartElement("numberedItems");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			this.XmlOutput.WriteStartElement("paragraph");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(RightAligned Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Sections Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.XmlOutput.WriteElementString("separator", string.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			this.XmlOutput.WriteStartElement("table");

			foreach (MarkdownElement[] Row in Element.Headers)
				await this.Render(Row, true, Element);

			foreach (MarkdownElement[] Row in Element.Rows)
				await this.Render(Row, false, Element);

			this.XmlOutput.WriteEndElement();
		}

		private async Task Render(MarkdownElement[] Row, bool HeaderRow, Table Element)
		{
			int i, c = Row.Length;

			this.XmlOutput.WriteStartElement("row");

			for (i = 0; i < c; i++)
			{
				MarkdownElement Cell = Row[i];

				if (!(Cell is null))
				{
					int Span = 1;

					while (i + 1 < c && Row[i + 1] is null)
					{
						i++;
						Span++;
					}

					this.XmlOutput.WriteStartElement("cell");
					this.XmlOutput.WriteAttributeString("alignment", Element.Alignments[i].ToString());
					this.XmlOutput.WriteAttributeString("colSpan", Span.ToString());
					this.XmlOutput.WriteAttributeString("header", CommonTypes.Encode(HeaderRow));

					await Cell.Render(this);

					this.XmlOutput.WriteEndElement();
				}
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.XmlOutput.WriteStartElement("row");
			this.XmlOutput.WriteStartElement("cell");
			this.XmlOutput.WriteAttributeString("alignment", "Center");
			this.XmlOutput.WriteAttributeString("colSpan", "1");
			this.XmlOutput.WriteAttributeString("header", "false");
			this.XmlOutput.WriteElementString("text", Element.IsChecked ? "✓" : " ");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteStartElement("cell");
			this.XmlOutput.WriteAttributeString("alignment", "Left");
			this.XmlOutput.WriteAttributeString("colSpan", "1");
			this.XmlOutput.WriteAttributeString("header", "false");

			await this.RenderChild(Element);

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			this.XmlOutput.WriteStartElement("table");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.XmlOutput.WriteStartElement("item");

			if (Element.Child is Paragraph P)
				await this.RenderChildren(P);
			else
				await this.RenderChild(Element);

			this.XmlOutput.WriteEndElement();
		}

		#endregion

	}
}
