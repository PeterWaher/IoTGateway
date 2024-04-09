using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.JavaScript
{
	/// <summary>
	/// Renders JavaScript from a Markdown document.
	/// </summary>
	public class JavaScriptRenderer : Renderer
	{
		/// <summary>
		/// HTML settings.
		/// </summary>
		public readonly HtmlSettings htmlSettings;

		/// <summary>
		/// Renders JavaScript from a Markdown document.
		/// </summary>
		/// <param name="HtmlSettings">HTML settings.</param>
		public JavaScriptRenderer(HtmlSettings HtmlSettings)
			: base()
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders JavaScript from a Markdown document.
		/// </summary>
		/// <param name="Output">HTML output.</param>
		/// <param name="HtmlSettings">HTML settings.</param>
		public JavaScriptRenderer(StringBuilder Output, HtmlSettings HtmlSettings)
			: base(Output)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders JavaScript from a Markdown document.
		/// </summary>
		/// <param name="HtmlSettings">HTML settings.</param>
		/// <param name="Document">Document being rendered.</param>
		public JavaScriptRenderer(HtmlSettings HtmlSettings, MarkdownDocument Document)
			: base(Document)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders JavaScript from a Markdown document.
		/// </summary>
		/// <param name="Output">HTML output.</param>
		/// <param name="HtmlSettings">HTML settings.</param>
		/// <param name="Document">Document being rendered.</param>
		public JavaScriptRenderer(StringBuilder Output, HtmlSettings HtmlSettings, MarkdownDocument Document)
			: base(Output, Document)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			string FileName = Path.GetFileName(this.Document.FileName);
			bool LetterOrUnderscore = false;
			StringBuilder sb = new StringBuilder();

			foreach (char ch in Path.ChangeExtension(FileName, null))
			{
				if (char.IsLetter(ch) || ch == '_')
				{
					sb.Append(ch);
					LetterOrUnderscore = true;
				}
				else if (char.IsDigit(ch))
				{
					if (!LetterOrUnderscore)
					{
						sb.Append('_');
						LetterOrUnderscore = true;
					}

					sb.Append(ch);
				}
				else
				{
					sb.Append('_');
					LetterOrUnderscore = true;
				}
			}

			string Name = sb.ToString();

			this.Output.Append("function CreateInnerHTML");
			this.Output.Append(Name);
			this.Output.AppendLine("(ElementId)");
			this.Output.AppendLine("{");
			this.Output.AppendLine("\tvar Element = document.getElementById(ElementId);");
			this.Output.AppendLine("\tif (Element)");
			this.Output.Append("\t\tElement.innerHTML = CreateHTML");
			this.Output.Append(Name);
			this.Output.AppendLine("();");
			this.Output.AppendLine("}");
			this.Output.AppendLine();
			this.Output.Append("function CreateHTML");
			this.Output.Append(Name);
			this.Output.AppendLine("()");
			this.Output.AppendLine("{");
			this.Output.Append("\tvar Segments = [");

			this.firstSegment = true;

			return Task.CompletedTask;
		}

		private bool firstSegment;

		private void AppendHtml(string Html)
		{
			this.AppendHtml(Html, false, false);
		}

		private void AppendHtml(string Html, bool Script, bool AppendNewLine)
		{
			if (this.firstSegment)
				this.firstSegment = false;
			else
				this.Output.Append(',');

			this.Output.AppendLine();
			this.Output.Append("\t\t");

			if (Script)
				this.Output.Append(Html);
			else
			{
				this.Output.Append('"');
				this.Output.Append(JSON.Encode(Html));

				if (AppendNewLine)
					this.Output.Append(jsonEncodedNewLine);

				this.Output.Append('"');
			}
		}

		private void AppendHtmlLine()
		{
			this.AppendHtml(string.Empty, false, true);
		}

		private void AppendHtmlLine(string Html)
		{
			this.AppendHtml(Html, false, true);
		}

		private static readonly string jsonEncodedNewLine = JSON.Encode(Environment.NewLine);

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			this.AppendHtmlLine("<div class=\"footnotes\">");
			this.AppendHtmlLine("<hr />");
			this.AppendHtmlLine("<ol>");

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if ((this.Document?.TryGetFootnoteNumber(Key, out int Nr) ?? false) &&
					(this.Document?.TryGetFootnote(Key, out Footnote Footnote) ?? false) &&
					Footnote.Referenced)
				{
					this.AppendHtml("<li id=\"fn-" + Nr.ToString() + "\">");

					if (!Footnote.BacklinkAdded)
					{
						InlineHTML Backlink = new InlineHTML(this.Document, "<a href=\"#fnref-" + Nr.ToString() + "\" class=\"footnote-backref\">&#8617;</a>");

						if (Footnote.LastChild is Paragraph P)
							P.AddChildren(Backlink);
						else
							Footnote.AddChildren(Backlink);

						Footnote.BacklinkAdded = true;
					}

					await Footnote.Render(this);

					this.AppendHtmlLine("</li>");
				}
			}

			this.AppendHtmlLine("</ol>");
			this.AppendHtmlLine("</div>");
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
			this.Output.AppendLine("];");
			this.Output.AppendLine("\treturn Segments.join(\"\");");
			this.Output.AppendLine("}");

			return Task.CompletedTask;
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Abbreviation Element)
		{
			this.AppendHtml("<abbr data-title=\"" +
				XML.HtmlAttributeEncode(Element.Description).Replace(" ", "&nbsp;") + "\">");

			await this.RenderChildren(Element);

			this.AppendHtml("</abbr>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkMail Element)
		{
			string s = Element.EMail;
			byte[] Data = Encoding.ASCII.GetBytes(s);
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Data)
			{
				sb.Append("&#x");
				sb.Append(b.ToString("X2"));
				sb.Append(';');
			}

			s = sb.ToString();

			sb.Clear();
			Data = Encoding.ASCII.GetBytes("mailto:");
			foreach (byte b in Data)
			{
				sb.Append("&#x");
				sb.Append(b.ToString("X2"));
				sb.Append(';');
			}

			this.AppendHtml("<a href=\"" + sb.ToString() + s + "\">" + s + "</a>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			bool IsRelative = Element.URL.IndexOf(':') < 0;

			this.AppendHtml("<a href=\"" +
				XML.HtmlAttributeEncode(this.Document.CheckURL(Element.URL, null)));

			if (!IsRelative)
				this.AppendHtml("\" target=\"_blank");

			this.AppendHtml("\">" + XML.HtmlValueEncode(Element.URL) + "</a>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.AppendHtml("<del>");
			await this.RenderChildren(Element);
			this.AppendHtml("</del>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DetailsReference Element)
		{
			if (!(this.Document.Detail is null))
				return this.RenderDocument(this.Document.Detail, true);
			else
				return this.Render((MetaReference)Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(EmojiReference Element)
		{
			IEmojiSource EmojiSource = this.Document.EmojiSource;

			if (EmojiSource is null)
				this.AppendHtml(Element.Delimiter + Element.Emoji.ShortName + Element.Delimiter);
			else if (!EmojiSource.EmojiSupported(Element.Emoji))
				this.AppendHtml(Element.Emoji.Unicode);
			else
			{
				StringBuilder sb = new StringBuilder();
				EmojiSource.GenerateHTML(sb, Element.Emoji, Element.Level, this.Document.Settings.EmbedEmojis);
				this.AppendHtml(sb.ToString());
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.AppendHtml("<em>");
			await this.RenderChildren(Element);
			this.AppendHtml("</em>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			string s;

			if (!(this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false))
				Footnote = null;

			if (Element.AutoExpand && !(Footnote is null))
				await this.Render(Footnote);
			else if (this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false)
			{
				s = Nr.ToString();

				this.AppendHtml("<sup id=\"fnref-" + s + "\"><a href=\"#fn-" + s +
					"\" class=\"footnote-ref\">" + s + "</a></sup>");

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
			this.AppendHtml("<mark");

			string s = this.htmlSettings?.HashtagClass;

			if (!string.IsNullOrEmpty(s))
				this.AppendHtml(" class=\"" + XML.HtmlAttributeEncode(s) + "\"");

			s = this.htmlSettings?.HashtagClickScript;

			if (!string.IsNullOrEmpty(s))
				this.AppendHtml(" onclick=\"" + XML.HtmlAttributeEncode(s) + "\"");

			this.AppendHtml(">" + Element.Tag + "</mark>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
		{
			if (this.htmlSettings?.XmlEntitiesOnly ?? true)
			{
				switch (Element.Entity)
				{
					case "quot":
					case "amp":
					case "apos":
					case "lt":
					case "gt":
					case "nbsp":    // Has syntactic significance when parsing HTML or Markdown
						this.AppendHtml("&" + Element.Entity + ";");
						break;

					case "QUOT":
					case "AMP":
					case "LT":
					case "GT":
						this.AppendHtml("&" + Element.Entity.ToLower() + ";");
						break;

					default:
						string s = Html.HtmlEntity.EntityToCharacter(Element.Entity);
						if (!string.IsNullOrEmpty(s))
							this.AppendHtml(s);
						break;
				}
			}
			else
				this.AppendHtml("&" + Element.Entity + ";");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.AppendHtml("&#" + Element.Code.ToString() + ";");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.AppendHtml("<code>" + XML.HtmlValueEncode(Element.Code) + "</code>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			this.AppendHtml(Element.HTML);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineScript Element)
		{
			this.AppendHtml(Element.Expression.Script, true, false);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates HTML from Script output.
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
					this.AppendHtml("<figure>");

				this.AppendHtml("<img border=\"2\" width=\"" + GraphSettings.Width.ToString() +
					"\" height=\"" + GraphSettings.Height.ToString() +
					"\" src=\"data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length) +
					"\" alt=\"");

				if (G is Graph2D Graph2D && !string.IsNullOrEmpty(Graph2D.Title))
					this.AppendHtml(XML.Encode(Graph2D.Title));
				else
					this.AppendHtml("Graph");

				this.AppendHtml("\" />");

				if (AloneInParagraph)
					this.AppendHtml("</figure>");
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					this.AppendHtml("<figure>");

				this.AppendHtml("<img border=\"2\" width=\"" + Pixels.Width.ToString() +
					"\" height=\"" + Pixels.Height.ToString() + "\" src=\"data:image/png;base64," +
					Convert.ToBase64String(Bin, 0, Bin.Length) + "\" alt=\"Image\" />");

				if (AloneInParagraph)
					this.AppendHtml("</figure>");
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					if (AloneInParagraph)
						this.AppendHtml("<figure>");

					this.AppendHtml("<img border=\"2\" width=\"" + Img.Width.ToString() +
						"\" height=\"" + Img.Height.ToString() + "\" src=\"data:image/png;base64," +
						Convert.ToBase64String(Bin, 0, Bin.Length) + "\" alt=\"Image\" />");

					if (AloneInParagraph)
						this.AppendHtml("</figure>");
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
				ex = Log.UnnestException(ex);

				this.AppendHtmlLine("<font class=\"error\">");

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
						this.AppendHtmlLine("<p>" + XML.HtmlValueEncode(ex3.Message) + "</p>");
				}
				else
				{
					if (AloneInParagraph)
						this.AppendHtml("<p>");

					this.AppendHtml(XML.HtmlValueEncode(ex.Message));

					if (AloneInParagraph)
						this.AppendHtml("</p>");
				}

				this.AppendHtmlLine("</font>");
			}
			else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
			{
				this.AppendHtml("<table><thead><tr>");

				foreach (string s2 in M.ColumnNames)
					this.AppendHtml("<th>" + FormatText(XML.HtmlValueEncode(s2)) + "</th>");

				this.AppendHtml("</tr></thead><tbody>");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					this.AppendHtml("<tr>");

					for (x = 0; x < M.Columns; x++)
					{
						this.AppendHtml("<td>");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (!(Item is null))
						{
							if (Item is string s2)
								this.AppendHtml(FormatText(XML.HtmlValueEncode(s2)));
							else if (Item is MarkdownElement Element)
								await Element.Render(this);
							else
								this.AppendHtml(FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
						}

						this.AppendHtml("</td>");
					}

					this.AppendHtml("</tr>");
				}

				this.AppendHtml("</tbody></table>");
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await this.RenderObject(Item, false, Variables);
			}
			else
			{
				if (AloneInParagraph)
					this.AppendHtml("<p>");

				this.AppendHtml(XML.HtmlValueEncode(Result?.ToString() ?? string.Empty));

				if (AloneInParagraph)
					this.AppendHtml("</p>");
			}

			if (AloneInParagraph)
				this.AppendHtmlLine();
		}

		private static string FormatText(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>").
				Replace("\t", "&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.AppendHtml(XML.HtmlValueEncode(Element.Value));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.AppendHtml("<ins>");
			await this.RenderChildren(Element);
			this.AppendHtml("</ins>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.AppendHtmlLine("<br/>");

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
		/// Generates HTML for a link.
		/// </summary>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="Document">Markdown document.</param>
		public async Task Render(string Url, string Title, IEnumerable<MarkdownElement> ChildNodes, MarkdownDocument Document)
		{
			bool IsRelative = Url.IndexOf(':') < 0;

			if (!IsRelative && Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
			{
				this.AppendHtml("<a href=\"#\" onclick=\"" +
					XML.HtmlAttributeEncode(System.Net.WebUtility.UrlDecode(Url.Substring(11))));

				IsRelative = true;
			}
			else
				this.AppendHtml("<a href=\"" + XML.HtmlAttributeEncode(Document.CheckURL(Url, null)));

			if (!string.IsNullOrEmpty(Title))
				this.AppendHtml("\" title=\"" + XML.HtmlAttributeEncode(Title));

			if (!IsRelative)
				this.AppendHtml("\" target=\"_blank");

			this.AppendHtml("\">");

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(this);

			this.AppendHtml("</a>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LinkReference Element)
		{
			Multimedia Multimedia = this.Document.GetReference(Element.Label);

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
						this.AppendHtml(" ");

					this.AppendHtml(XML.HtmlValueEncode(P.Key));
					if (P.Value)
					{
						this.AppendHtmlLine("<br/>");
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
		public override async Task Render(Multimedia Element)
		{
			IMultimediaHtmlRenderer Renderer = Element.MultimediaHandler<IMultimediaHtmlRenderer>();
			if (Renderer is null)
				await this.RenderChildren(Element);
			else
			{
				using (HtmlRenderer Renderer2 = new HtmlRenderer(this.htmlSettings))
				{
					await Renderer.RenderHtml(Renderer2, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
					this.AppendHtml(Renderer2.ToString());
				}
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MultimediaReference Element)
		{
			Multimedia Multimedia = Element.Document.GetReference(Element.Label);

			if (!(Multimedia is null))
			{
				IMultimediaHtmlRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaHtmlRenderer>();
				if (!(Renderer is null))
				{
					using (HtmlRenderer Renderer2 = new HtmlRenderer(this.htmlSettings))
					{
						await Renderer.RenderHtml(Renderer2, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
						this.AppendHtml(Renderer2.ToString());
					}
				}
			}

			await this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.AppendHtml("<s>");
			await this.RenderChildren(Element);
			this.AppendHtml("</s>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.AppendHtml("<strong>");
			await this.RenderChildren(Element);
			this.AppendHtml("</strong>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.AppendHtml("<sub>");
			await this.RenderChildren(Element);
			this.AppendHtml("</sub>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.AppendHtml("<sup>");
			await this.RenderChildren(Element);
			this.AppendHtml("</sup>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.AppendHtml("<u>");
			await this.RenderChildren(Element);
			this.AppendHtml("</u>");
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			this.AppendHtmlLine("<blockquote>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</blockquote>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			this.AppendHtmlLine("<ul>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</ul>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			this.AppendHtmlLine("<div class='horizontalAlignCenter'>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</div>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			ICodeContentHtmlRenderer Renderer = Element.CodeContentHandler<ICodeContentHtmlRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					using (HtmlRenderer Renderer2 = new HtmlRenderer(this.htmlSettings))
					{
						if (await Renderer.RenderHtml(Renderer2, Element.Rows, Element.Language, Element.Indent, Element.Document))
							return;

						this.AppendHtml(Renderer2.ToString());
					}
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					this.AppendHtmlLine("<font class=\"error\">");

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
							this.AppendHtmlLine("<p>" + XML.HtmlValueEncode(ex3.Message) + "</p>");
					}
					else
						this.AppendHtml("<p>" + XML.HtmlValueEncode(ex.Message) + "</p>");

					this.AppendHtml("</font>");
				}
			}

			int i;

			this.AppendHtml("<pre><code class=\"");

			if (string.IsNullOrEmpty(Element.Language))
				this.AppendHtml("nohighlight");
			else
				this.AppendHtml(XML.Encode(Element.Language));

			this.AppendHtml("\">");

			for (i = Element.Start; i <= Element.End; i++)
				this.AppendHtmlLine(Element.IndentString + XML.HtmlValueEncode(Element.Rows[i]));

			this.AppendHtmlLine("</code></pre>");
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
			foreach (MarkdownElement E in Element.Children)
			{
				this.AppendHtml("<dd>");
				await E.Render(this);
				this.AppendHtmlLine("</dd>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionList Element)
		{
			this.AppendHtmlLine("<dl>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</dl>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionTerms Element)
		{
			foreach (MarkdownElement E in Element.Children)
			{
				this.AppendHtml("<dt>");
				await E.Render(this);
				this.AppendHtmlLine("</dt>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			this.AppendHtmlLine("<blockquote class=\"deleted\">");

			foreach (MarkdownElement E in Element.Children)
				await E.Render(this);

			this.AppendHtmlLine("</blockquote>");
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
			this.AppendHtml("<h" + Element.Level.ToString());

			string Id = await Element.Id;

			if (!string.IsNullOrEmpty(Id))
				this.AppendHtml(" id=\"" + XML.HtmlAttributeEncode(Id) + "\"");

			if (Element.Document.IncludesTableOfContents)
				this.AppendHtml(" class=\"tocReference\"");

			this.AppendHtml(">");

			await this.RenderChildren(Element);

			this.AppendHtmlLine("</h" + Element.Level.ToString() + ">");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.AppendHtmlLine("<hr/>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			await this.RenderChildren(Element);
			this.AppendHtmlLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			this.AppendHtmlLine("<blockquote class=\"inserted\">");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</blockquote>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InvisibleBreak Element)
		{
			return Task.CompletedTask; // TODO
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(LeftAligned Element)
		{
			this.AppendHtmlLine("<div class='horizontalAlignLeft'>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</div>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			this.AppendHtmlLine("<div class='horizontalAlignMargins'>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</div>");
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
			if (Element.NumberExplicit)
				this.AppendHtml("<li value=\"" + Element.Number.ToString() + "\">");
			else
				this.AppendHtml("<li>");

			await this.RenderChild(Element);

			this.AppendHtmlLine("</li>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			NumberedItem Item;
			int Expected = 0;

			this.AppendHtmlLine("<ol>");

			foreach (MarkdownElement E in Element.Children)
			{
				Expected++;
				Item = E as NumberedItem;

				if (Item is null)
					await E.Render(this);
				else if (Item.Number == Expected)
				{
					this.AppendHtml("<li>");
					await Item.Child.Render(this);
					this.AppendHtmlLine("</li>");
				}
				else
				{
					await Item.Render(this);
					Expected = Item.Number;
				}
			}

			this.AppendHtmlLine("</ol>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			if (!Element.Implicit)
				this.AppendHtml("<p>");

			await this.RenderChildren(Element);

			if (!Element.Implicit)
				this.AppendHtmlLine("</p>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(RightAligned Element)
		{
			this.AppendHtmlLine("<div class='horizontalAlignRight'>");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</div>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Sections Element)
		{
			this.GenerateSectionHTML(Element.InitialNrColumns);
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</section>");
		}

		private void GenerateSectionHTML(int NrColumns)
		{
			this.AppendHtml("<section");

			if (NrColumns > 1)
			{
				string s = NrColumns.ToString();

				this.AppendHtml(" style=\"-webkit-column-count:" + s + ";-moz-column-count:" +
					s + ";column-count:" + s + "\"");
			}

			this.AppendHtmlLine(">");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.AppendHtmlLine("</section>");
			this.GenerateSectionHTML(Element.NrColumns);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			MarkdownElement E;
			int i, j, k;

			this.AppendHtmlLine("<table>");

			if (!string.IsNullOrEmpty(Element.Id))
			{
				this.AppendHtml("<caption id=\"" + XML.HtmlAttributeEncode(Element.Id) + "\">");

				if (string.IsNullOrEmpty(Element.Caption))
					this.AppendHtml(XML.HtmlValueEncode(Element.Id));
				else
					this.AppendHtml(XML.HtmlValueEncode(Element.Caption));

				this.AppendHtmlLine("</caption>");
			}

			this.AppendHtmlLine("<colgroup>");

			foreach (TextAlignment Alignment in Element.Alignments)
				this.AppendHtmlLine("<col style=\"text-align:" + Alignment.ToString().ToLower() + "\"/>");

			this.AppendHtmlLine("</colgroup>");

			this.AppendHtmlLine("<thead>");
			foreach (MarkdownElement[] Row in Element.Headers)
			{
				this.AppendHtmlLine("<tr>");

				for (i = 0; i < Element.Columns; i++)
				{
					E = Row[i];
					if (E is null)
						continue;

					k = 1;
					j = i + 1;
					while (j < Element.Columns && Row[j++] is null)
						k++;

					this.AppendHtml("<th style=\"text-align:" +
						Element.Alignments[i].ToString().ToLower());

					if (k > 1)
						this.AppendHtml("\" colspan=\"" + k.ToString());

					this.AppendHtml("\">");
					await E.Render(this);
					this.AppendHtmlLine("</th>");
				}

				this.AppendHtmlLine("</tr>");
			}
			this.AppendHtmlLine("</thead>");

			this.AppendHtmlLine("<tbody>");
			foreach (MarkdownElement[] Row in Element.Rows)
			{
				this.AppendHtmlLine("<tr>");

				for (i = 0; i < Element.Columns; i++)
				{
					E = Row[i];
					if (E is null)
						continue;

					k = 1;
					j = i + 1;
					while (j < Element.Columns && Row[j++] is null)
						k++;

					this.AppendHtml("<td style=\"text-align:" +
						Element.Alignments[i].ToString().ToLower());

					if (k > 1)
						this.AppendHtml("\" colspan=\"" + k.ToString());

					this.AppendHtml("\">");
					await E.Render(this);
					this.AppendHtmlLine("</td>");
				}

				this.AppendHtmlLine("</tr>");
			}
			this.AppendHtmlLine("</tbody>");

			this.AppendHtmlLine("</table>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.AppendHtml("<li class=\"taskListItem\"><input disabled=\"disabled");

			if (Element.CheckPosition > 0)
			{
				this.AppendHtml("\" id=\"item" + Element.CheckPosition.ToString() +
					"\" data-position=\"" + Element.CheckPosition.ToString());
			}

			this.AppendHtml("\" type=\"checkbox\"");

			if (Element.IsChecked)
				this.AppendHtml(" checked=\"checked\"");

			this.AppendHtml("/><span></span><label class=\"taskListItemLabel\"");

			if (Element.CheckPosition > 0)
				this.AppendHtml(" for=\"item" + Element.CheckPosition.ToString() + "\"");

			this.AppendHtml(">");

			if (Element.Child is NestedBlock NestedBlock)
			{
				bool EndLabel = true;
				bool First = true;

				foreach (MarkdownElement E in NestedBlock.Children)
				{
					if (First)
					{
						First = false;

						if (E.InlineSpanElement)
							await E.Render(this);
						else
						{
							await NestedBlock.Render(this);
							break;
						}
					}
					else
					{
						if (!E.InlineSpanElement)
						{
							this.AppendHtml("</label>");
							EndLabel = false;
						}

						await E.Render(this);
					}
				}

				if (EndLabel)
					this.AppendHtml("</label>");

				this.AppendHtmlLine("</li>");
			}
			else
			{
				await Element.Child.Render(this);
				this.AppendHtmlLine("</label></li>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			this.AppendHtmlLine("<ul class=\"taskList\">");
			await this.RenderChildren(Element);
			this.AppendHtmlLine("</ul>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.AppendHtml("<li");

			MarkdownDocument Detail = Element.Document.Detail;

			if (!(Detail is null))
			{
				if (Element.Child is Link Link)
				{
					if (string.Compare(Detail.ResourceName, Link.Url, true) == 0)
						this.AppendHtml(" class=\"active\"");
				}
				else if (Element.Child is LinkReference LinkReference)
				{
					string Label = LinkReference.Label;
					Multimedia Multimedia = Element.Document.GetReference(Label);

					if (!(Multimedia is null) && Multimedia.Items.Length == 1 &&
						string.Compare(Multimedia.Items[0].Url, Detail.ResourceName, true) == 0)
					{
						this.AppendHtml(" class=\"active\"");
					}
				}
			}

			this.AppendHtml(">");
			await Element.Child.Render(this);
			this.AppendHtmlLine("</li>");
		}

		#endregion

	}
}
