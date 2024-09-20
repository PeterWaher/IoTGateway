using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Renders HTML from a Markdown document.
	/// </summary>
	public class HtmlRenderer : Renderer
	{
		/// <summary>
		/// HTML settings.
		/// </summary>
		public readonly HtmlSettings htmlSettings;

		/// <summary>
		/// Renders HTML from a Markdown document.
		/// </summary>
		/// <param name="HtmlSettings">HTML settings.</param>
		public HtmlRenderer(HtmlSettings HtmlSettings)
			: base()
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders HTML from a Markdown document.
		/// </summary>
		/// <param name="Output">HTML output.</param>
		/// <param name="HtmlSettings">HTML settings.</param>
		public HtmlRenderer(StringBuilder Output, HtmlSettings HtmlSettings)
			: base(Output)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders HTML from a Markdown document.
		/// </summary>
		/// <param name="HtmlSettings">HTML settings.</param>
		/// <param name="Document">Document being rendered.</param>
		public HtmlRenderer(HtmlSettings HtmlSettings, MarkdownDocument Document)
			: base(Document)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders HTML from a Markdown document.
		/// </summary>
		/// <param name="Output">HTML output.</param>
		/// <param name="HtmlSettings">HTML settings.</param>
		/// <param name="Document">Document being rendered.</param>
		public HtmlRenderer(StringBuilder Output, HtmlSettings HtmlSettings, MarkdownDocument Document)
			: base(Output, Document)
		{
			this.htmlSettings = HtmlSettings;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			StringBuilder sb = null;
			string Title;
			string Description;
			string s2;
			bool First;

			this.Output.AppendLine("<!DOCTYPE html>");
			this.Output.AppendLine("<html itemscope itemtype=\"http://schema.org/WebPage\">");
			this.Output.AppendLine("<head>");

			if (this.Document.TryGetMetaData("TITLE", out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (sb is null)
						sb = new StringBuilder();
					else
						sb.Append(' ');

					sb.Append(P.Key);
				}

				if (this.Document.TryGetMetaData("SUBTITLE", out Values))
				{
					sb.Append(" -");
					foreach (KeyValuePair<string, bool> P in Values)
					{
						sb.Append(' ');
						sb.Append(P.Key);
					}
				}

				Title = XML.HtmlAttributeEncode(sb.ToString());
				sb = null;

				this.Output.Append("<title>");
				if (string.IsNullOrEmpty(Title))
					this.Output.Append(' ');
				else
					this.Output.Append(Title);
				this.Output.AppendLine("</title>");

				this.Output.Append("<meta itemprop=\"name\" content=\"");
				this.Output.Append(Title);
				this.Output.AppendLine("\"/>");

				this.Output.Append("<meta name=\"twitter:title\" content=\"");
				this.Output.Append(Title);
				this.Output.AppendLine("\"/>");

				this.Output.Append("<meta name=\"og:title\" content=\"");
				this.Output.Append(Title);
				this.Output.AppendLine("\"/>");
			}
			else
				this.Output.AppendLine("<title> </title>");

			if (this.Document.TryGetMetaData("DESCRIPTION", out Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (sb is null)
						sb = new StringBuilder();
					else
						sb.Append(' ');

					sb.Append(P.Key);
				}

				if (!(sb is null))
				{
					Description = XML.HtmlAttributeEncode(sb.ToString());

					this.Output.Append("<meta itemprop=\"description\" content=\"");
					this.Output.Append(Description);
					this.Output.AppendLine("\"/>");

					this.Output.Append("<meta name=\"twitter:description\" content=\"");
					this.Output.Append(Description);
					this.Output.AppendLine("\"/>");

					this.Output.Append("<meta name=\"og:description\" content=\"");
					this.Output.Append(Description);
					this.Output.AppendLine("\"/>");
				}
			}

			if (this.Document.TryGetMetaData("AUTHOR", out Values))
			{
				if (sb is null || string.IsNullOrEmpty(s2 = sb.ToString()))
					sb = new StringBuilder("Author:");
				else
				{
					char ch = s2[s2.Length - 1];

					if (!char.IsPunctuation(ch))
						sb.Append(',');

					sb.Append(" Author:");
				}

				foreach (KeyValuePair<string, bool> P in Values)
				{
					sb.Append(' ');
					sb.Append(P.Key);
				}
			}

			if (this.Document.TryGetMetaData("DATE", out Values))
			{
				if (sb is null || string.IsNullOrEmpty(s2 = sb.ToString()))
					sb = new StringBuilder("Date:");
				else
				{
					char ch = s2[s2.Length - 1];

					if (!char.IsPunctuation(ch))
						sb.Append(',');

					sb.Append(" Date:");
				}

				foreach (KeyValuePair<string, bool> P in Values)
				{
					sb.Append(' ');
					sb.Append(P.Key);
				}
			}

			if (!(sb is null))
			{
				this.Output.Append("<meta name=\"description\" content=\"");
				this.Output.Append(XML.HtmlAttributeEncode(sb.ToString()));
				this.Output.AppendLine("\"/>");
			}

			foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> MetaData in this.Document.MetaData)
			{
				switch (MetaData.Key)
				{
					case "ACCESS-CONTROL-ALLOW-ORIGIN":
					case "ALLOWSCRIPTTAG":
					case "ALTERNATE":
					case "AUDIOAUTOPLAY":
					case "AUDIOCONTROLS":
					case "BODYONLY":
					case "CONTENT-SECURITY-POLICY":
					case "COPYRIGHT":
					case "CACHE-CONTROL":
					case "CSS":
					case "DATE":
					case "DESCRIPTION":
					case "HELP":
					case "ICON":
					case "INIT":
					case "JAVASCRIPT":
					case "LOGIN":
					case "MASTER":
					case "NEXT":
					case "PARAMETER":
					case "PUBLIC-KEY-PINS":
					case "PREV":
					case "PREVIOUS":
					case "PRIVILEGE":
					case "SCRIPT":
					case "STRICT-TRANSPORT-SECURITY":
					case "SUBTITLE":
					case "SUNSET":
					case "TITLE":
					case "USERVARIABLE":
					case "VARY":
					case "VIDEOAUTOPLAY":
					case "VIDEOCONTROLS":
						break;

					case "KEYWORDS":
						First = true;
						this.Output.Append("<meta name=\"keywords\" content=\"");

						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							if (First)
								First = false;
							else
								this.Output.Append(", ");

							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
						}

						this.Output.AppendLine("\"/>");
						break;

					case "AUTHOR":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<meta name=\"author\" content=\"");
							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "IMAGE":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							s2 = XML.HtmlAttributeEncode(P.Key);

							this.Output.Append("<meta itemprop=\"image\" content=\"");
							this.Output.Append(s2);
							this.Output.AppendLine("\"/>");

							this.Output.Append("<meta name=\"twitter:image\" content=\"");
							this.Output.Append(s2);
							this.Output.AppendLine("\"/>");

							this.Output.Append("<meta name=\"og:image\" content=\"");
							this.Output.Append(s2);
							this.Output.AppendLine("\"/>");
						}
						break;

					case "WEB":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<meta name=\"og:url\" content=\"");
							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "REFRESH":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<meta http-equiv=\"refresh\" content=\"");
							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "VIEWPORT":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<meta name=\"viewport\" content=\"");
							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
							this.Output.AppendLine("\"/>");
						}
						break;

					default:
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<meta name=\"");
							this.Output.Append(XML.HtmlAttributeEncode(MetaData.Key));
							this.Output.Append("\" content=\"");
							this.Output.Append(XML.HtmlAttributeEncode(P.Key));
							this.Output.AppendLine("\"/>");
						}
						break;
				}
			}

			bool HighlightStyle = false;

			foreach (KeyValuePair<string, KeyValuePair<string, bool>[]> MetaData in this.Document.MetaData)
			{
				switch (MetaData.Key)
				{
					case "COPYRIGHT":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<link rel=\"copyright\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "PREVIOUS":
					case "PREV":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{

							this.Output.Append("<link rel=\"prev\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "NEXT":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<link rel=\"next\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "ALTERNATE":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<link rel=\"alternate\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "HELP":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<link rel=\"help\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "ICON":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<link rel=\"shortcut icon\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "CSS":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							s2 = this.Document.CheckURL(P.Key, null);
							if (s2.StartsWith("/Highlight/styles/", StringComparison.OrdinalIgnoreCase))
								HighlightStyle = true;

							this.Output.Append("<link rel=\"stylesheet\" href=\"");
							this.Output.Append(XML.HtmlAttributeEncode(s2));
							this.Output.AppendLine("\"/>");
						}
						break;

					case "JAVASCRIPT":
						foreach (KeyValuePair<string, bool> P in MetaData.Value)
						{
							this.Output.Append("<script type=\"application/javascript\" src=\"");
							this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(P.Key, null)));
							this.Output.AppendLine("\"></script>");
						}
						break;
				}
			}

			if (this.Document.SyntaxHighlighting)
			{
				if (!HighlightStyle)
					this.Output.AppendLine("<link rel=\"stylesheet\" href=\"/highlight/styles/default.css\">");

				this.Output.AppendLine("<script src=\"/highlight/highlight.pack.js\"></script>");
				this.Output.AppendLine("<script>hljs.initHighlightingOnLoad();</script>");
			}

			this.Output.AppendLine("</head>");
			this.Output.AppendLine("<body>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			this.Output.AppendLine("<div class=\"footnotes\">");
			this.Output.AppendLine("<hr />");
			this.Output.AppendLine("<ol>");

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if ((this.Document?.TryGetFootnoteNumber(Key, out int Nr) ?? false) &&
					(this.Document?.TryGetFootnote(Key, out Footnote Footnote) ?? false) &&
					Footnote.Referenced)
				{
					this.Output.Append("<li id=\"fn-");
					this.Output.Append(Nr.ToString());
					this.Output.Append("\">");

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

					this.Output.AppendLine("</li>");
				}
			}

			this.Output.AppendLine("</ol>");
			this.Output.AppendLine("</div>");
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
			this.Output.AppendLine("</body>");
			this.Output.Append("</html>");

			return Task.CompletedTask;
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Abbreviation Element)
		{
			this.Output.Append("<abbr data-title=\"");
			this.Output.Append(XML.HtmlAttributeEncode(Element.Description).Replace(" ", "&nbsp;"));
			this.Output.Append("\">");

			await this.RenderChildren(Element);

			this.Output.Append("</abbr>");
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

			this.Output.Append("<a href=\"");
			this.Output.Append(sb.ToString());
			this.Output.Append(s);
			this.Output.Append("\">");
			this.Output.Append(s);
			this.Output.Append("</a>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			bool IsRelative = Element.URL.IndexOf(':') < 0;

			this.Output.Append("<a href=\"");
			this.Output.Append(XML.HtmlAttributeEncode(this.Document.CheckURL(Element.URL, null)));

			if (!IsRelative)
				this.Output.Append("\" target=\"_blank");

			this.Output.Append("\">");
			this.Output.Append(XML.HtmlValueEncode(Element.URL));
			this.Output.Append("</a>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.Output.Append("<del>");
			await this.RenderChildren(Element);
			this.Output.Append("</del>");
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
			{
				this.Output.Append(Element.Delimiter);
				this.Output.Append(Element.Emoji.ShortName);
				this.Output.Append(Element.Delimiter);
			}
			else if (!EmojiSource.EmojiSupported(Element.Emoji))
				this.Output.Append(Element.Emoji.Unicode);
			else
				EmojiSource.GenerateHTML(this.Output, Element.Emoji, Element.Level, this.Document.Settings.EmbedEmojis);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.Output.Append("<em>");
			await this.RenderChildren(Element);
			this.Output.Append("</em>");
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

				this.Output.Append("<sup id=\"fnref-");
				this.Output.Append(s);
				this.Output.Append("\"><a href=\"#fn-");
				this.Output.Append(s);
				this.Output.Append("\" class=\"footnote-ref\">");
				this.Output.Append(s);
				this.Output.Append("</a></sup>");

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
			this.Output.Append("<mark");

			string s = this.htmlSettings?.HashtagClass;

			if (!string.IsNullOrEmpty(s))
			{
				this.Output.Append(" class=\"");
				this.Output.Append(XML.HtmlAttributeEncode(s));
				this.Output.Append('"');
			}

			s = this.htmlSettings?.HashtagClickScript;

			if (!string.IsNullOrEmpty(s))
			{
				this.Output.Append(" onclick=\"");
				this.Output.Append(XML.HtmlAttributeEncode(s));
				this.Output.Append('"');
			}

			this.Output.Append('>');
			this.Output.Append(Element.Tag);
			this.Output.Append("</mark>");

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
						this.Output.Append('&');
						this.Output.Append(Element.Entity);
						this.Output.Append(';');
						break;

					case "QUOT":
					case "AMP":
					case "LT":
					case "GT":
						this.Output.Append('&');
						this.Output.Append(Element.Entity.ToLower());
						this.Output.Append(';');
						break;

					default:
						string s = Html.HtmlEntity.EntityToCharacter(Element.Entity);
						if (!string.IsNullOrEmpty(s))
							this.Output.Append(s);
						break;
				}
			}
			else
			{
				this.Output.Append('&');
				this.Output.Append(Element.Entity);
				this.Output.Append(';');
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.Output.Append("&#");
			this.Output.Append(Element.Code.ToString());
			this.Output.Append(';');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.Output.Append("<code>");
			this.Output.Append(XML.HtmlValueEncode(Element.Code));
			this.Output.Append("</code>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			this.Output.Append(Element.HTML);

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
					this.Output.Append("<figure>");

				this.Output.Append("<img border=\"2\" width=\"");
				this.Output.Append(GraphSettings.Width.ToString());
				this.Output.Append("\" height=\"");
				this.Output.Append(GraphSettings.Height.ToString());
				this.Output.Append("\" src=\"data:image/png;base64,");
				this.Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
				this.Output.Append("\" alt=\"");

				if (G is Graph2D Graph2D && !string.IsNullOrEmpty(Graph2D.Title))
					this.Output.Append(XML.Encode(Graph2D.Title));
				else
					this.Output.Append("Graph");

				this.Output.Append("\" />");

				if (AloneInParagraph)
					this.Output.Append("</figure>");
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					this.Output.Append("<figure>");

				this.Output.Append("<img border=\"2\" width=\"");
				this.Output.Append(Pixels.Width.ToString());
				this.Output.Append("\" height=\"");
				this.Output.Append(Pixels.Height.ToString());
				this.Output.Append("\" src=\"data:image/png;base64,");
				this.Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
				this.Output.Append("\" alt=\"Image\" />");

				if (AloneInParagraph)
					this.Output.Append("</figure>");
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					if (AloneInParagraph)
						this.Output.Append("<figure>");

					this.Output.Append("<img border=\"2\" width=\"");
					this.Output.Append(Img.Width.ToString());
					this.Output.Append("\" height=\"");
					this.Output.Append(Img.Height.ToString());
					this.Output.Append("\" src=\"data:image/png;base64,");
					this.Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
					this.Output.Append("\" alt=\"Image\" />");

					if (AloneInParagraph)
						this.Output.Append("</figure>");
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

				this.Output.AppendLine("<font class=\"error\">");

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						this.Output.Append("<p>");
						this.Output.Append(XML.HtmlValueEncode(ex3.Message));
						this.Output.AppendLine("</p>");
					}
				}
				else
				{
					if (AloneInParagraph)
						this.Output.Append("<p>");

					this.Output.Append(XML.HtmlValueEncode(ex.Message));

					if (AloneInParagraph)
						this.Output.Append("</p>");
				}

				this.Output.AppendLine("</font>");
			}
			else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
			{
				this.Output.Append("<table><thead><tr>");

				foreach (string s2 in M.ColumnNames)
				{
					this.Output.Append("<th>");
					this.Output.Append(FormatText(XML.HtmlValueEncode(s2)));
					this.Output.Append("</th>");
				}

				this.Output.Append("</tr></thead><tbody>");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					this.Output.Append("<tr>");

					for (x = 0; x < M.Columns; x++)
					{
						this.Output.Append("<td>");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (!(Item is null))
						{
							if (Item is string s2)
								this.Output.Append(FormatText(XML.HtmlValueEncode(s2)));
							else if (Item is MarkdownElement Element)
								await Element.Render(this);
							else
								this.Output.Append(FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
						}

						this.Output.Append("</td>");
					}

					this.Output.Append("</tr>");
				}

				this.Output.Append("</tbody></table>");
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await this.RenderObject(Item, false, Variables);
			}
			else
			{
				if (AloneInParagraph)
					this.Output.Append("<p>");

				this.Output.Append(XML.HtmlValueEncode(Result?.ToString() ?? string.Empty));

				if (AloneInParagraph)
					this.Output.Append("</p>");
			}

			if (AloneInParagraph)
				this.Output.AppendLine();
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
			this.Output.Append(XML.HtmlValueEncode(Element.Value));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.Output.Append("<ins>");
			await this.RenderChildren(Element);
			this.Output.Append("</ins>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.Output.AppendLine("<br/>");

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
				this.Output.Append("<a href=\"#\" onclick=\"");
				this.Output.Append(XML.HtmlAttributeEncode(System.Net.WebUtility.UrlDecode(Url.Substring(11))));
				IsRelative = true;
			}
			else
			{
				this.Output.Append("<a href=\"");
				this.Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Url, null)));
			}

			if (!string.IsNullOrEmpty(Title))
			{
				this.Output.Append("\" title=\"");
				this.Output.Append(XML.HtmlAttributeEncode(Title));
			}

			if (!IsRelative)
				this.Output.Append("\" target=\"_blank");

			this.Output.Append("\">");

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(this);

			this.Output.Append("</a>");
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
						this.Output.Append(' ');

					this.Output.Append(XML.HtmlValueEncode(P.Key));
					if (P.Value)
					{
						this.Output.AppendLine("<br/>");
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
			IMultimediaHtmlRenderer Renderer = Element.MultimediaHandler<IMultimediaHtmlRenderer>();
			if (Renderer is null)
				return this.RenderChildren(Element);
			else
				return Renderer.RenderHtml(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
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
				IMultimediaHtmlRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaHtmlRenderer>();
				if (!(Renderer is null))
					return Renderer.RenderHtml(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.Output.Append("<s>");
			await this.RenderChildren(Element);
			this.Output.Append("</s>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.Output.Append("<strong>");
			await this.RenderChildren(Element);
			this.Output.Append("</strong>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.Output.Append("<sub>");
			await this.RenderChildren(Element);
			this.Output.Append("</sub>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.Output.Append("<sup>");
			await this.RenderChildren(Element);
			this.Output.Append("</sup>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.Output.Append("<u>");
			await this.RenderChildren(Element);
			this.Output.Append("</u>");
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			this.Output.AppendLine("<blockquote>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</blockquote>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			this.Output.AppendLine("<ul>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</ul>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			this.Output.AppendLine("<div class='horizontalAlignCenter'>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</div>");
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
					if (await Renderer.RenderHtml(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					this.Output.AppendLine("<font class=\"error\">");

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							this.Output.Append("<p>");
							this.Output.Append(XML.HtmlValueEncode(ex3.Message));
							this.Output.AppendLine("</p>");
						}
					}
					else
					{
						this.Output.Append("<p>");
						this.Output.Append(XML.HtmlValueEncode(ex.Message));
						this.Output.Append("</p>");
					}

					this.Output.Append("</font>");
				}
			}

			int i;

			this.Output.Append("<pre><code class=\"");

			if (string.IsNullOrEmpty(Element.Language))
				this.Output.Append("nohighlight");
			else
				this.Output.Append(XML.Encode(Element.Language));

			this.Output.Append("\">");

			for (i = Element.Start; i <= Element.End; i++)
			{
				this.Output.Append(Element.IndentString);
				this.Output.AppendLine(XML.HtmlValueEncode(Element.Rows[i]));
			}

			this.Output.AppendLine("</code></pre>");
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
				this.Output.Append("<dd>");
				await E.Render(this);
				this.Output.AppendLine("</dd>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionList Element)
		{
			this.Output.AppendLine("<dl>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</dl>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionTerms Element)
		{
			foreach (MarkdownElement E in Element.Children)
			{
				this.Output.Append("<dt>");
				await E.Render(this);
				this.Output.AppendLine("</dt>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			this.Output.AppendLine("<blockquote class=\"deleted\">");

			foreach (MarkdownElement E in Element.Children)
				await E.Render(this);

			this.Output.AppendLine("</blockquote>");
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
			this.Output.Append("<h");
			this.Output.Append(Element.Level.ToString());

			string Id = await Element.Id;

			if (!string.IsNullOrEmpty(Id))
			{
				this.Output.Append(" id=\"");
				this.Output.Append(XML.HtmlAttributeEncode(Id));
				this.Output.Append("\"");
			}

			if (Element.Document.IncludesTableOfContents)
				this.Output.Append(" class=\"tocReference\"");

			this.Output.Append('>');

			await this.RenderChildren(Element);

			this.Output.Append("</h");
			this.Output.Append(Element.Level.ToString());
			this.Output.AppendLine(">");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.Output.AppendLine("<hr/>");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			await this.RenderChildren(Element);
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			this.Output.AppendLine("<blockquote class=\"inserted\">");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</blockquote>");
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
			this.Output.AppendLine("<div class='horizontalAlignLeft'>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</div>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			this.Output.AppendLine("<div class='horizontalAlignMargins'>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</div>");
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
			{
				this.Output.Append("<li value=\"");
				this.Output.Append(Element.Number.ToString());
				this.Output.Append("\">");
			}
			else
				this.Output.Append("<li>");

			await this.RenderChild(Element);

			this.Output.AppendLine("</li>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			NumberedItem Item;
			int Expected = 0;

			this.Output.AppendLine("<ol>");

			foreach (MarkdownElement E in Element.Children)
			{
				Expected++;
				Item = E as NumberedItem;

				if (Item is null)
					await E.Render(this);
				else if (Item.Number == Expected)
				{
					this.Output.Append("<li>");
					await Item.Child.Render(this);
					this.Output.AppendLine("</li>");
				}
				else
				{
					await Item.Render(this);
					Expected = Item.Number;
				}
			}

			this.Output.AppendLine("</ol>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			if (!Element.Implicit)
				this.Output.Append("<p>");

			await this.RenderChildren(Element);

			if (!Element.Implicit)
				this.Output.AppendLine("</p>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(RightAligned Element)
		{
			this.Output.AppendLine("<div class='horizontalAlignRight'>");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</div>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Sections Element)
		{
			this.GenerateSectionHTML(Element.InitialNrColumns);
			await this.RenderChildren(Element);
			this.Output.AppendLine("</section>");
		}

		private void GenerateSectionHTML(int NrColumns)
		{
			this.Output.Append("<section");

			if (NrColumns > 1)
			{
				string s = NrColumns.ToString();

				this.Output.Append(" style=\"-webkit-column-count:");
				this.Output.Append(s);
				this.Output.Append(";-moz-column-count:");
				this.Output.Append(s);
				this.Output.Append(";column-count:");
				this.Output.Append(s);
				this.Output.Append('"');
			}

			this.Output.AppendLine(">");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.Output.AppendLine("</section>");
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
			MarkdownElement[] Row;
			TextAlignment?[] CellAlignments;
			int NrRows, RowIndex;
			int NrColumns = Element.Columns;
			int i, j, k;

			this.Output.AppendLine("<table>");

			if (!string.IsNullOrEmpty(Element.Id))
			{
				this.Output.Append("<caption id=\"");
				this.Output.Append(XML.HtmlAttributeEncode(Element.Id));
				this.Output.Append("\">");

				if (string.IsNullOrEmpty(Element.Caption))
					this.Output.Append(XML.HtmlValueEncode(Element.Id));
				else
					this.Output.Append(XML.HtmlValueEncode(Element.Caption));

				this.Output.AppendLine("</caption>");
			}

			this.Output.AppendLine("<colgroup>");
			foreach (TextAlignment Alignment in Element.ColumnAlignments)
			{
				this.Output.Append("<col style=\"text-align:");
				this.Output.Append(Alignment.ToString().ToLower());
				this.Output.AppendLine("\"/>");
			}
			this.Output.AppendLine("</colgroup>");

			this.Output.AppendLine("<thead>");

			NrRows = Element.Headers.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Headers[RowIndex];
				CellAlignments = Element.HeaderCellAlignments[RowIndex];

				this.Output.AppendLine("<tr>");

				for (i = 0; i < NrColumns; i++)
				{
					E = Row[i];
					if (E is null)
						continue;

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					this.Output.Append("<th style=\"text-align:");
					this.Output.Append((CellAlignments[i] ?? Element.ColumnAlignments[i]).ToString().ToLower());

					if (k > 1)
					{
						this.Output.Append("\" colspan=\"");
						this.Output.Append(k.ToString());
					}

					this.Output.Append("\">");
					await E.Render(this);
					this.Output.AppendLine("</th>");
				}

				this.Output.AppendLine("</tr>");
			}
			this.Output.AppendLine("</thead>");

			this.Output.AppendLine("<tbody>");

			NrRows = Element.Rows.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Rows[RowIndex];
				CellAlignments = Element.RowCellAlignments[RowIndex];

				this.Output.AppendLine("<tr>");

				for (i = 0; i < NrColumns; i++)
				{
					E = Row[i];
					if (E is null)
						continue;

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					this.Output.Append("<td style=\"text-align:");
					this.Output.Append((CellAlignments[i] ?? Element.ColumnAlignments[i]).ToString().ToLower());

					if (k > 1)
					{
						this.Output.Append("\" colspan=\"");
						this.Output.Append(k.ToString());
					}

					this.Output.Append("\">");
					await E.Render(this);
					this.Output.AppendLine("</td>");
				}

				this.Output.AppendLine("</tr>");
			}
			this.Output.AppendLine("</tbody>");

			this.Output.AppendLine("</table>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.Output.Append("<li class=\"taskListItem\"><input disabled=\"disabled");

			if (Element.CheckPosition > 0)
			{
				this.Output.Append("\" id=\"item");
				this.Output.Append(Element.CheckPosition.ToString());
				this.Output.Append("\" data-position=\"");
				this.Output.Append(Element.CheckPosition.ToString());
			}

			this.Output.Append("\" type=\"checkbox\"");

			if (Element.IsChecked)
				this.Output.Append(" checked=\"checked\"");

			this.Output.Append("/><span></span><label class=\"taskListItemLabel\"");

			if (Element.CheckPosition > 0)
			{
				this.Output.Append(" for=\"item");
				this.Output.Append(Element.CheckPosition.ToString());
				this.Output.Append("\"");
			}

			this.Output.Append('>');

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
							this.Output.Append("</label>");
							EndLabel = false;
						}

						await E.Render(this);
					}
				}

				if (EndLabel)
					this.Output.Append("</label>");

				this.Output.AppendLine("</li>");
			}
			else
			{
				await Element.Child.Render(this);
				this.Output.AppendLine("</label></li>");
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			this.Output.AppendLine("<ul class=\"taskList\">");
			await this.RenderChildren(Element);
			this.Output.AppendLine("</ul>");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.Output.Append("<li");

			MarkdownDocument Detail = Element.Document.Detail;

			if (!(Detail is null))
			{
				if (Element.Child is Link Link)
				{
					if (string.Compare(Detail.ResourceName, Link.Url, true) == 0)
						this.Output.Append(" class=\"active\"");
				}
				else if (Element.Child is LinkReference LinkReference)
				{
					string Label = LinkReference.Label;
					Model.SpanElements.Multimedia Multimedia = Element.Document.GetReference(Label);

					if (!(Multimedia is null) && Multimedia.Items.Length == 1 &&
						string.Compare(Multimedia.Items[0].Url, Detail.ResourceName, true) == 0)
					{
						this.Output.Append(" class=\"active\"");
					}
				}
			}

			this.Output.Append('>');
			await Element.Child.Render(this);
			this.Output.AppendLine("</li>");
		}

		#endregion

	}
}
