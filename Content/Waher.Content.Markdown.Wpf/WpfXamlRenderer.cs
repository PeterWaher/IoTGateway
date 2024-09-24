using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.Multimedia;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Wpf
{
	/// <summary>
	/// Renders XAML (WPF flavour) from a Markdown document.
	/// </summary>
	public class WpfXamlRenderer : Renderer
	{
		/// <summary>
		/// XML output
		/// </summary>
		public readonly XmlWriter XmlOutput;

		/// <summary>
		/// XAML settings.
		/// </summary>
		public readonly XamlSettings XamlSettings;

		/// <summary>
		/// Current text-alignment.
		/// </summary>
		public TextAlignment Alignment = TextAlignment.Left;

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		public WpfXamlRenderer()
			: this(new XamlSettings())
		{
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		public WpfXamlRenderer(XmlWriterSettings XmlSettings)
			: this(XmlSettings, new XamlSettings())
		{
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="XamlSettings">XAML-specific settings.</param>
		public WpfXamlRenderer(XamlSettings XamlSettings)
			: this(XML.WriterSettings(false, true), XamlSettings)
		{
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="XamlSettings">XAML-specific settings.</param>
		public WpfXamlRenderer(XmlWriterSettings XmlSettings, XamlSettings XamlSettings)
			: base()
		{
			this.XamlSettings = XamlSettings;
			this.XmlOutput = XmlWriter.Create(this.Output, XmlSettings);
		}

		/// <summary>
		/// Renders XAML (WPF flavour) from a Markdown document.
		/// </summary>
		/// <param name="Output">XAML output.</param>
		/// <param name="XmlSettings">XML-specific settings.</param>
		/// <param name="XamlSettings">XAML-specific settings.</param>
		public WpfXamlRenderer(StringBuilder Output, XmlWriterSettings XmlSettings, XamlSettings XamlSettings)
			: base(Output)
		{
			this.XamlSettings = XamlSettings;
			this.XmlOutput = XmlWriter.Create(this.Output, XmlSettings);
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
		public override Task RenderDocument(MarkdownDocument Document, bool Inclusion)
		{
			this.Alignment = TextAlignment.Left;

			return base.RenderDocument(Document, Inclusion);
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			this.XmlOutput.WriteStartElement("StackPanel", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			this.XmlOutput.WriteAttributeString("xmlns", "x", null, "http://schemas.microsoft.com/winfx/2006/xaml");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			Footnote Footnote;
			string FootnoteMargin = "0," + this.XamlSettings.ParagraphMarginTop.ToString() + "," +
				this.XamlSettings.FootnoteSeparator.ToString() + "," +
				this.XamlSettings.ParagraphMarginBottom.ToString();
			string Scale = CommonTypes.Encode(this.XamlSettings.SuperscriptScale);
			string Offset = this.XamlSettings.SuperscriptOffset.ToString();
			int Nr;
			int Row = 0;

			this.XmlOutput.WriteElementString("Separator", string.Empty);

			this.XmlOutput.WriteStartElement("Grid");
			this.XmlOutput.WriteStartElement("Grid.ColumnDefinitions");

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "Auto");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "*");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("Grid.RowDefinitions");

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if ((this.Document?.TryGetFootnoteNumber(Key, out Nr) ?? false) &&
					(this.Document?.TryGetFootnote(Key, out Footnote) ?? false) &&
					Footnote.Referenced)
				{
					this.XmlOutput.WriteStartElement("RowDefinition");
					this.XmlOutput.WriteAttributeString("Height", "Auto");
					this.XmlOutput.WriteEndElement();
				}
			}

			this.XmlOutput.WriteEndElement();

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if ((this.Document?.TryGetFootnoteNumber(Key, out Nr) ?? false) &&
					(this.Document?.TryGetFootnote(Key, out Footnote) ?? false) &&
					Footnote.Referenced)
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("Text", Nr.ToString());
					this.XmlOutput.WriteAttributeString("Margin", FootnoteMargin);
					this.XmlOutput.WriteAttributeString("Grid.Column", "0");
					this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());

					this.XmlOutput.WriteStartElement("TextBlock.LayoutTransform");
					this.XmlOutput.WriteStartElement("TransformGroup");

					this.XmlOutput.WriteStartElement("ScaleTransform");
					this.XmlOutput.WriteAttributeString("ScaleX", Scale);
					this.XmlOutput.WriteAttributeString("ScaleY", Scale);
					this.XmlOutput.WriteEndElement();

					this.XmlOutput.WriteStartElement("TranslateTransform");
					this.XmlOutput.WriteAttributeString("Y", Offset);
					this.XmlOutput.WriteEndElement();

					this.XmlOutput.WriteEndElement();
					this.XmlOutput.WriteEndElement();
					this.XmlOutput.WriteEndElement();

					if (Footnote.InlineSpanElement && !Footnote.OutsideParagraph)
					{
						this.XmlOutput.WriteStartElement("TextBlock");
						this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					}
					else
						this.XmlOutput.WriteStartElement("StackPanel");

					this.XmlOutput.WriteAttributeString("Grid.Column", "1");
					this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());

					TextAlignment Bak = this.Alignment;
					this.Alignment = TextAlignment.Left;

					await Footnote.Render(this);
					this.Alignment = Bak;

					this.XmlOutput.WriteEndElement();

					Row++;
				}
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
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
			this.XmlOutput.WriteStartElement("Hyperlink");
			this.XmlOutput.WriteAttributeString("NavigateUri", "mailto:" + Element.EMail);
			this.XmlOutput.WriteValue(Element.EMail);
			this.XmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.XmlOutput.WriteStartElement("Hyperlink");
			this.XmlOutput.WriteAttributeString("NavigateUri", this.Document.CheckURL(Element.URL, null));
			this.XmlOutput.WriteValue(Element.URL);
			this.XmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");

			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			this.XmlOutput.WriteStartElement("TextBlock.TextDecorations");
			this.XmlOutput.WriteStartElement("TextDecoration");
			this.XmlOutput.WriteAttributeString("Location", "Strikethrough");
			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DetailsReference Element)
		{
			if (!(this.Document.Detail is null))
				return this.RenderDocument(this.Document.Detail, false);
			else
				return this.Render((MetaReference)Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(EmojiReference Element)
		{
			IEmojiSource EmojiSource = this.Document.EmojiSource;

			if (EmojiSource is null)
			{
				this.XmlOutput.WriteValue(Element.Delimiter);
				this.XmlOutput.WriteValue(Element.Emoji.ShortName);
				this.XmlOutput.WriteValue(Element.Delimiter);
			}
			else if (!EmojiSource.EmojiSupported(Element.Emoji))
				this.XmlOutput.WriteValue(Element.Emoji.Unicode);
			else
			{
				IImageSource Source = await EmojiSource.GetImageSource(Element.Emoji, Element.Level);
				await Multimedia.ImageContent.OutputWpf(this.XmlOutput, Source, Element.Emoji.Description);
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.XmlOutput.WriteStartElement("Italic");
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
				string s;

				this.XmlOutput.WriteStartElement("TextBlock");
				this.XmlOutput.WriteAttributeString("Text", Nr.ToString());

				this.XmlOutput.WriteStartElement("TextBlock.LayoutTransform");
				this.XmlOutput.WriteStartElement("TransformGroup");

				this.XmlOutput.WriteStartElement("ScaleTransform");
				this.XmlOutput.WriteAttributeString("ScaleX", s = CommonTypes.Encode(this.XamlSettings.SuperscriptScale));
				this.XmlOutput.WriteAttributeString("ScaleY", s);
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteStartElement("TranslateTransform");
				this.XmlOutput.WriteAttributeString("Y", this.XamlSettings.SuperscriptOffset.ToString());
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
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
			this.XmlOutput.WriteValue(Element.Tag);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
		{
			string s = Html.HtmlEntity.EntityToCharacter(Element.Entity);
			if (s is null)
				this.XmlOutput.WriteRaw("&" + Element.Entity + ";");
			else
				this.XmlOutput.WriteValue(s);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.XmlOutput.WriteValue(new string((char)Element.Code, 1));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("xml", "space", null, "preserve");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
			this.XmlOutput.WriteAttributeString("FontFamily", "Courier New");
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			this.XmlOutput.WriteValue(Element.Code);

			this.XmlOutput.WriteEndElement();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			this.XmlOutput.WriteComment(Element.HTML);

			return Task.CompletedTask;
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
		/// Generates WPF XAML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		public async Task RenderObject(object Result, bool AloneInParagraph, Variables Variables)
		{
			if (Result is null)
				return;

			string s;

			if (Result is XmlDocument Xml)
				Result = await MarkdownDocument.TransformXml(Xml, Variables);
			else if (Result is IToMatrix ToMatrix)
				Result = ToMatrix.ToMatrix();

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels();
				byte[] Bin = Pixels.EncodeAsPng();

				s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

				await Multimedia.ImageContent.OutputWpf(this.XmlOutput, new ImageSource()
				{
					Url = s,
					Width = Pixels.Width,
					Height = Pixels.Height
				}, string.Empty);
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

					await Multimedia.ImageContent.OutputWpf(this.XmlOutput, new ImageSource()
					{
						Url = s,
						Width = Img.Width,
						Height = Img.Height
					}, string.Empty);
				}
			}
			else if (Result is MarkdownDocument Doc)
			{
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is MarkdownContent Markdown)
			{
				Doc = await MarkdownDocument.CreateAsync(Markdown.Markdown, Markdown.Settings ?? new MarkdownSettings());
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						this.XmlOutput.WriteStartElement("TextBlock");
						this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
						this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);

						if (this.Alignment != TextAlignment.Left)
							this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

						this.XmlOutput.WriteAttributeString("Foreground", "Red");
						this.XmlOutput.WriteValue(ex3.Message);
						this.XmlOutput.WriteEndElement();
					}
				}
				else
				{
					if (AloneInParagraph)
					{
						this.XmlOutput.WriteStartElement("TextBlock");
						this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
						this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
						if (this.Alignment != TextAlignment.Left)
							this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());
					}
					else
						this.XmlOutput.WriteStartElement("Run");

					this.XmlOutput.WriteAttributeString("Foreground", "Red");
					this.XmlOutput.WriteValue(ex.Message);
					this.XmlOutput.WriteEndElement();
				}
			}
			else
			{
				if (AloneInParagraph)
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());
				}

				this.XmlOutput.WriteValue(Result.ToString());

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
			this.XmlOutput.WriteValue(Element.Value);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.XmlOutput.WriteStartElement("Underline");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.XmlOutput.WriteElementString("LineBreak", string.Empty);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Link Element)
		{
			return this.Render(Element.Url, Element.Title, Element.Children, Element.Document);
		}

		/// <summary>
		/// Generates XAML for a link.
		/// </summary>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="Document">Markdown document.</param>
		public async Task Render(string Url, string Title, IEnumerable<MarkdownElement> ChildNodes, MarkdownDocument Document)
		{
			this.XmlOutput.WriteStartElement("Hyperlink");
			this.XmlOutput.WriteAttributeString("NavigateUri", Document.CheckURL(Url, null));

			if (!string.IsNullOrEmpty(Title))
				this.XmlOutput.WriteAttributeString("ToolTip", Title);

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(this);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LinkReference Element)
		{
			Model.SpanElements.Multimedia Multimedia = this.Document.GetReference(Element.Label);

			if (!(Multimedia is null))
				return this.Render(Multimedia.Items[0].Url, Multimedia.Items[0].Title, Element.Children, Element.Document);
			else
				return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MetaReference Element)
		{
			bool FirstOnRow = true;

			if (Element.TryGetMetaData(out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						this.XmlOutput.WriteValue(' ');

					this.XmlOutput.WriteValue(P.Key);
					if (P.Value)
					{
						this.XmlOutput.WriteElementString("LineBreak", string.Empty);
						FirstOnRow = true;
					}
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Model.SpanElements.Multimedia Element)
		{
			IMultimediaWpfXamlRenderer Renderer = Element.MultimediaHandler<IMultimediaWpfXamlRenderer>();
			if (Renderer is null)
				return this.RenderChildren(Element);
			else
				return Renderer.RenderWpfXaml(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
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
				IMultimediaWpfXamlRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaWpfXamlRenderer>();
				if (!(Renderer is null))
					return Renderer.RenderWpfXaml(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			this.XmlOutput.WriteStartElement("TextBlock.TextDecorations");
			this.XmlOutput.WriteStartElement("TextDecoration");
			this.XmlOutput.WriteAttributeString("Location", "Strikethrough");
			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.XmlOutput.WriteStartElement("Bold");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.XmlOutput.WriteStartElement("Run");
			this.XmlOutput.WriteAttributeString("Typography.Variants", "Subscript");

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.XmlOutput.WriteStartElement("Run");
			this.XmlOutput.WriteAttributeString("Typography.Variants", "Superscript");

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.XmlOutput.WriteStartElement("Underline");
			await this.RenderChildren(Element);
			this.XmlOutput.WriteEndElement();
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			this.XmlOutput.WriteStartElement("Border");
			this.XmlOutput.WriteAttributeString("BorderThickness", this.XamlSettings.BlockQuoteBorderThickness.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.BlockQuoteMargin.ToString() + "," +
				this.XamlSettings.ParagraphMarginTop.ToString() + ",0," + this.XamlSettings.ParagraphMarginBottom.ToString());
			this.XmlOutput.WriteAttributeString("Padding", this.XamlSettings.BlockQuotePadding.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("BorderBrush", this.XamlSettings.BlockQuoteBorderColor);
			this.XmlOutput.WriteStartElement("StackPanel");

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			int Row = 0;
			bool ParagraphBullet;

			this.XmlOutput.WriteStartElement("Grid");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			this.XmlOutput.WriteStartElement("Grid.ColumnDefinitions");

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "Auto");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "*");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("Grid.RowDefinitions");

			foreach (MarkdownElement _ in Element.Children)
			{
				this.XmlOutput.WriteStartElement("RowDefinition");
				this.XmlOutput.WriteAttributeString("Height", "Auto");
				this.XmlOutput.WriteEndElement();
			}

			this.XmlOutput.WriteEndElement();

			foreach (MarkdownElement E in Element.Children)
			{
				ParagraphBullet = !E.InlineSpanElement || E.OutsideParagraph;
				this.GetMargins(E, out int TopMargin, out int BottomMargin);

				this.XmlOutput.WriteStartElement("TextBlock");
				this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
				this.XmlOutput.WriteAttributeString("Grid.Column", "0");
				this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());
				if (this.Alignment != TextAlignment.Left)
					this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

				this.XmlOutput.WriteAttributeString("Margin", "0," + TopMargin.ToString() + "," +
					this.XamlSettings.ListContentMargin.ToString() + "," + BottomMargin.ToString());

				this.XmlOutput.WriteValue("•");
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteStartElement("StackPanel");
				this.XmlOutput.WriteAttributeString("Grid.Column", "1");
				this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());

				if (ParagraphBullet)
					await E.Render(this);
				else
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

					await E.Render(this);

					this.XmlOutput.WriteEndElement();
				}

				this.XmlOutput.WriteEndElement();

				Row++;
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="Element">Element to render.</param>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		private void GetMargins(MarkdownElement Element, out int TopMargin, out int BottomMargin)
		{
			if (Element.InlineSpanElement && !Element.OutsideParagraph)
			{
				TopMargin = 0;
				BottomMargin = 0;
			}
			else if (Element is NestedBlock NestedBlock)
			{
				bool First = true;

				TopMargin = BottomMargin = 0;

				foreach (MarkdownElement E in NestedBlock.Children)
				{
					if (First)
					{
						First = false;
						this.GetMargins(E, out TopMargin, out BottomMargin);
					}
					else
						this.GetMargins(E, out int _, out BottomMargin);
				}
			}
			else if (Element is MarkdownElementSingleChild SingleChild)
				this.GetMargins(SingleChild.Child, out TopMargin, out BottomMargin);
			else
			{
				TopMargin = this.XamlSettings.ParagraphMarginTop;
				BottomMargin = this.XamlSettings.ParagraphMarginBottom;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			this.XmlOutput.WriteStartElement("StackPanel");

			TextAlignment Bak = this.Alignment;
			this.Alignment = TextAlignment.Center;

			await this.RenderChildren(Element);

			this.Alignment = Bak;
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			ICodeContentWpfXamlRenderer Renderer = Element.CodeContentHandler<ICodeContentWpfXamlRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					if (await Renderer.RenderWpfXaml(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							this.XmlOutput.WriteStartElement("TextBlock");
							this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
							this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);

							if (this.Alignment != TextAlignment.Left)
								this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

							this.XmlOutput.WriteAttributeString("Foreground", "Red");
							this.XmlOutput.WriteValue(ex3.Message);
							this.XmlOutput.WriteEndElement();
						}
					}
					else
					{
						this.XmlOutput.WriteStartElement("TextBlock");
						this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
						this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
						if (this.Alignment != TextAlignment.Left)
							this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

						this.XmlOutput.WriteAttributeString("Foreground", "Red");
						this.XmlOutput.WriteValue(ex.Message);
						this.XmlOutput.WriteEndElement();
					}
				}
			}

			int i;
			bool First = true;

			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("xml", "space", null, "preserve");
			this.XmlOutput.WriteAttributeString("TextWrapping", "NoWrap");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			this.XmlOutput.WriteAttributeString("FontFamily", "Courier New");
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			for (i = Element.Start; i <= Element.End; i++)
			{
				if (First)
					First = false;
				else
					this.XmlOutput.WriteElementString("LineBreak", string.Empty);

				this.XmlOutput.WriteValue(Element.Rows[i]);
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
		public override async Task Render(DefinitionDescriptions Element)
		{
			MarkdownElement Last = null;

			foreach (MarkdownElement Description in Element.Children)
				Last = Description;

			foreach (MarkdownElement Description in Element.Children)
			{
				if (Description.InlineSpanElement && !Description.OutsideParagraph)
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
				}
				else
					this.XmlOutput.WriteStartElement("StackPanel");

				this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.DefinitionMargin.ToString() + ",0,0," +
					(Description == Last ? this.XamlSettings.DefinitionSeparator : 0).ToString());

				await Description.Render(this);
				this.XmlOutput.WriteEndElement();
			}
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
		public override async Task Render(DefinitionTerms Element)
		{
			int TopMargin = this.XamlSettings.ParagraphMarginTop;

			foreach (MarkdownElement Term in Element.Children)
			{
				this.XmlOutput.WriteStartElement("TextBlock");
				this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
				this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMarginLeft.ToString() + "," +
					TopMargin.ToString() + "," + this.XamlSettings.ParagraphMarginRight.ToString() + ",0");
				this.XmlOutput.WriteAttributeString("FontWeight", "Bold");

				await Term.Render(this);

				this.XmlOutput.WriteEndElement();
				TopMargin = 0;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			this.XmlOutput.WriteStartElement("Border");
			this.XmlOutput.WriteAttributeString("BorderThickness", this.XamlSettings.BlockQuoteBorderThickness.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.BlockQuoteMargin.ToString() + "," +
				this.XamlSettings.ParagraphMarginTop.ToString() + ",0," + this.XamlSettings.ParagraphMarginBottom.ToString());
			this.XmlOutput.WriteAttributeString("Padding", this.XamlSettings.BlockQuotePadding.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("BorderBrush", this.XamlSettings.DeletedBlockQuoteBorderColor);
			this.XmlOutput.WriteStartElement("StackPanel");

			foreach (MarkdownElement E in Element.Children)
				await E.Render(this);

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();
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
			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			if (Element.Level > 0 && Element.Level <= this.XamlSettings.HeaderFontSize.Length)
			{
				this.XmlOutput.WriteAttributeString("FontSize", this.XamlSettings.HeaderFontSize[Element.Level - 1].ToString());
				this.XmlOutput.WriteAttributeString("Foreground", this.XamlSettings.HeaderForegroundColor[Element.Level - 1].ToString());
			}

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.XmlOutput.WriteElementString("Separator", string.Empty);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			this.XmlOutput.WriteStartElement("Border");
			this.XmlOutput.WriteAttributeString("BorderThickness", this.XamlSettings.BlockQuoteBorderThickness.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.BlockQuoteMargin.ToString() + "," +
				this.XamlSettings.ParagraphMarginTop.ToString() + ",0," + this.XamlSettings.ParagraphMarginBottom.ToString());
			this.XmlOutput.WriteAttributeString("Padding", this.XamlSettings.BlockQuotePadding.ToString() + ",0,0,0");
			this.XmlOutput.WriteAttributeString("BorderBrush", this.XamlSettings.InsertedBlockQuoteBorderColor);
			this.XmlOutput.WriteStartElement("StackPanel");

			await this.RenderChildren(Element);

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteEndElement();
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
		public override async Task Render(LeftAligned Element)
		{
			this.XmlOutput.WriteStartElement("StackPanel");

			TextAlignment Bak = this.Alignment;
			this.Alignment = TextAlignment.Left;

			await this.RenderChildren(Element);

			this.Alignment = Bak;
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			this.XmlOutput.WriteStartElement("StackPanel");

			TextAlignment Bak = this.Alignment;
			this.Alignment = TextAlignment.Left;

			await this.RenderChildren(Element);

			this.Alignment = Bak;
			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NestedBlock Element)
		{
			if (Element.HasOneChild)
				await Element.FirstChild.Render(this);
			else
			{
				bool SpanOpen = false;

				foreach (MarkdownElement E in Element.Children)
				{
					if (E.InlineSpanElement)
					{
						if (!SpanOpen)
						{
							this.XmlOutput.WriteStartElement("TextBlock");
							this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
							if (this.Alignment != TextAlignment.Left)
								this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());
							SpanOpen = true;
						}
					}
					else
					{
						if (SpanOpen)
						{
							this.XmlOutput.WriteEndElement();
							SpanOpen = false;
						}
					}

					await E.Render(this);
				}

				if (SpanOpen)
					this.XmlOutput.WriteEndElement();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(NumberedItem Element)
		{
			return this.RenderChild(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			NumberedItem Item;
			int Expected = 0;
			int Row = 0;
			bool ParagraphBullet;

			this.XmlOutput.WriteStartElement("Grid");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			this.XmlOutput.WriteStartElement("Grid.ColumnDefinitions");

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "Auto");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "*");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("Grid.RowDefinitions");

			foreach (MarkdownElement _ in Element.Children)
			{
				this.XmlOutput.WriteStartElement("RowDefinition");
				this.XmlOutput.WriteAttributeString("Height", "Auto");
				this.XmlOutput.WriteEndElement();
			}

			this.XmlOutput.WriteEndElement();

			foreach (MarkdownElement E in Element.Children)
			{
				Expected++;
				Item = E as NumberedItem;

				ParagraphBullet = !E.InlineSpanElement || E.OutsideParagraph;
				this.GetMargins(E, out int TopMargin, out int BottomMargin);

				this.XmlOutput.WriteStartElement("TextBlock");
				this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
				this.XmlOutput.WriteAttributeString("Grid.Column", "0");
				this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());
				if (this.Alignment != TextAlignment.Left)
					this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

				this.XmlOutput.WriteAttributeString("Margin", "0," + TopMargin.ToString() + "," +
					this.XamlSettings.ListContentMargin.ToString() + "," + BottomMargin.ToString());

				if (!(Item is null))
					this.XmlOutput.WriteValue((Expected = Item.Number).ToString());
				else
					this.XmlOutput.WriteValue(Expected.ToString());

				this.XmlOutput.WriteValue(".");
				this.XmlOutput.WriteEndElement();

				this.XmlOutput.WriteStartElement("StackPanel");
				this.XmlOutput.WriteAttributeString("Grid.Column", "1");
				this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());

				if (ParagraphBullet)
					await E.Render(this);
				else
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

					await E.Render(this);

					this.XmlOutput.WriteEndElement();
				}

				this.XmlOutput.WriteEndElement();

				Row++;
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			BaselineAlignment s;

			this.XmlOutput.WriteStartElement("TextBlock");
			this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			if (this.Alignment != TextAlignment.Left)
				this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

			foreach (MarkdownElement E in Element.Children)
			{
				if ((!E.InlineSpanElement || E.OutsideParagraph) && (s = E.BaselineAlignment) != BaselineAlignment.Baseline)
				{
					this.XmlOutput.WriteStartElement("InlineUIContainer");
					this.XmlOutput.WriteAttributeString("BaselineAlignment", s.ToString());

					await E.Render(this);

					this.XmlOutput.WriteEndElement();
				}
				else
					await E.Render(this);
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(RightAligned Element)
		{
			this.XmlOutput.WriteStartElement("StackPanel");

			TextAlignment Bak = this.Alignment;
			this.Alignment = TextAlignment.Right;

			await this.RenderChildren(Element);

			this.Alignment = Bak;
			this.XmlOutput.WriteEndElement();
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
			this.XmlOutput.WriteElementString("Separator", string.Empty);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			int Column;
			int Row, NrRows;
			int RowNr = 0;
			int NrColumns = Element.Columns;

			this.XmlOutput.WriteStartElement("Grid");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			if (!string.IsNullOrEmpty(Element.Caption))
				this.XmlOutput.WriteAttributeString("ToolTip", Element.Caption);

			this.XmlOutput.WriteStartElement("Grid.ColumnDefinitions");

			for (Column = 0; Column < NrColumns; Column++)
			{
				this.XmlOutput.WriteStartElement("ColumnDefinition");
				this.XmlOutput.WriteAttributeString("Width", "Auto");
				this.XmlOutput.WriteEndElement();
			}

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("Grid.RowDefinitions");

			for (Row = 0, NrRows = Element.Rows.Length + Element.Headers.Length; Row < NrRows; Row++)
			{
				this.XmlOutput.WriteStartElement("RowDefinition");
				this.XmlOutput.WriteAttributeString("Height", "Auto");
				this.XmlOutput.WriteEndElement();
			}

			this.XmlOutput.WriteEndElement();

			for (Row = 0, NrRows = Element.Headers.Length; Row < NrRows; Row++, RowNr++)
				await this.Render(Element.Headers[Row], Element.HeaderCellAlignments[Row], RowNr, true, Element);

			for (Row = 0, NrRows = Element.Rows.Length; Row < NrRows; Row++, RowNr++)
				await this.Render(Element.Rows[Row], Element.RowCellAlignments[Row], RowNr, false, Element);

			this.XmlOutput.WriteEndElement();
		}

		private async Task Render(MarkdownElement[] CurrentRow, TextAlignment?[] CellAlignments, int RowNr, bool Bold, Table Element)
		{
			TextAlignment Bak = this.Alignment;
			MarkdownElement E;
			int NrColumns = Element.Columns;
			int Column;
			int ColSpan;

			for (Column = 0; Column < NrColumns; Column++)
			{
				E = CurrentRow[Column];
				if (E is null)
					continue;

				this.Alignment = CellAlignments[Column] ?? Element.ColumnAlignments[Column];
				ColSpan = Column + 1;
				while (ColSpan < NrColumns && CurrentRow[ColSpan] is null)
					ColSpan++;

				ColSpan -= Column;

				this.XmlOutput.WriteStartElement("Border");
				this.XmlOutput.WriteAttributeString("BorderBrush", this.XamlSettings.TableCellBorderColor);
				this.XmlOutput.WriteAttributeString("BorderThickness", CommonTypes.Encode(this.XamlSettings.TableCellBorderThickness));

				if ((RowNr & 1) == 0)
				{
					if (!string.IsNullOrEmpty(this.XamlSettings.TableCellRowBackgroundColor1))
						this.XmlOutput.WriteAttributeString("Background", this.XamlSettings.TableCellRowBackgroundColor1);
				}
				else
				{
					if (!string.IsNullOrEmpty(this.XamlSettings.TableCellRowBackgroundColor2))
						this.XmlOutput.WriteAttributeString("Background", this.XamlSettings.TableCellRowBackgroundColor2);
				}

				this.XmlOutput.WriteAttributeString("Grid.Column", Column.ToString());
				this.XmlOutput.WriteAttributeString("Grid.Row", RowNr.ToString());

				if (ColSpan > 1)
					this.XmlOutput.WriteAttributeString("Grid.ColumnSpan", ColSpan.ToString());

				if (E.InlineSpanElement)
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					this.XmlOutput.WriteAttributeString("Padding", this.XamlSettings.TableCellPadding);

					if (Bold)
						this.XmlOutput.WriteAttributeString("FontWeight", "Bold");

					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());
				}
				else
				{
					this.XmlOutput.WriteStartElement("StackPanel");
					this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.TableCellPadding);
				}

				await E.Render(this);
				this.XmlOutput.WriteEndElement();
				this.XmlOutput.WriteEndElement();
			}

			this.Alignment = Bak;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(TaskItem Element)
		{
			return this.RenderChild(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			int Row = 0;
			bool ParagraphBullet;

			this.XmlOutput.WriteStartElement("Grid");
			this.XmlOutput.WriteAttributeString("Margin", this.XamlSettings.ParagraphMargins);
			this.XmlOutput.WriteStartElement("Grid.ColumnDefinitions");

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "Auto");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteStartElement("ColumnDefinition");
			this.XmlOutput.WriteAttributeString("Width", "*");
			this.XmlOutput.WriteEndElement();

			this.XmlOutput.WriteEndElement();
			this.XmlOutput.WriteStartElement("Grid.RowDefinitions");

			foreach (MarkdownElement _ in Element.Children)
			{
				this.XmlOutput.WriteStartElement("RowDefinition");
				this.XmlOutput.WriteAttributeString("Height", "Auto");
				this.XmlOutput.WriteEndElement();
			}

			this.XmlOutput.WriteEndElement();

			foreach (MarkdownElement E in Element.Children)
			{
				ParagraphBullet = !E.InlineSpanElement || E.OutsideParagraph;
				this.GetMargins(E, out int TopMargin, out int BottomMargin);

				if (E is TaskItem TaskItem && TaskItem.IsChecked)
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					this.XmlOutput.WriteAttributeString("Grid.Column", "0");
					this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());
					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

					this.XmlOutput.WriteAttributeString("Margin", "0," + TopMargin.ToString() + "," +
						this.XamlSettings.ListContentMargin.ToString() + "," + BottomMargin.ToString());

					this.XmlOutput.WriteValue("✓");
					this.XmlOutput.WriteEndElement();
				}

				this.XmlOutput.WriteStartElement("StackPanel");
				this.XmlOutput.WriteAttributeString("Grid.Column", "1");
				this.XmlOutput.WriteAttributeString("Grid.Row", Row.ToString());

				if (ParagraphBullet)
					await E.Render(this);
				else
				{
					this.XmlOutput.WriteStartElement("TextBlock");
					this.XmlOutput.WriteAttributeString("TextWrapping", "Wrap");
					if (this.Alignment != TextAlignment.Left)
						this.XmlOutput.WriteAttributeString("TextAlignment", this.Alignment.ToString());

					await E.Render(this);

					this.XmlOutput.WriteEndElement();
				}

				this.XmlOutput.WriteEndElement();

				Row++;
			}

			this.XmlOutput.WriteEndElement();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(UnnumberedItem Element)
		{
			return this.RenderChild(Element);
		}

		#endregion

	}
}
