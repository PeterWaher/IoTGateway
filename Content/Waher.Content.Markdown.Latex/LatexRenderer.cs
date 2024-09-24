using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Images;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.Multimedia;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Renders LaTeX from a Markdown document.
	/// </summary>
	public class LatexRenderer : Renderer
	{
		/// <summary>
		/// LaTeX settings.
		/// </summary>
		public readonly LaTeXSettings LatexSettings;

		/// <summary>
		/// Renders LaTeX from a Markdown document.
		/// </summary>
		/// <param name="LatexSettings">LaTeX settings.</param>
		public LatexRenderer(LaTeXSettings LatexSettings)
			: base()
		{
			this.LatexSettings = LatexSettings;
		}

		/// <summary>
		/// Renders LaTeX from a Markdown document.
		/// </summary>
		/// <param name="Output">LaTeX output.</param>
		/// <param name="LatexSettings">LaTeX settings.</param>
		public LatexRenderer(StringBuilder Output, LaTeXSettings LatexSettings)
			: base(Output)
		{
			this.LatexSettings = LatexSettings;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			bool MakeTitle = false;

			this.Output.Append("\\documentclass[");
			this.Output.Append(this.LatexSettings.DefaultFontSize.ToString());
			this.Output.Append("pt, ");

			switch (this.LatexSettings.PaperFormat)
			{
				case LaTeXPaper.A4:
				default:
					this.Output.Append("a4paper");
					break;

				case LaTeXPaper.Letter:
					this.Output.Append("letterpaper");
					break;
			}

			this.Output.Append("]{");
			switch (this.LatexSettings.DocumentClass)
			{
				case LaTeXDocumentClass.Article:
				default:
					this.Output.Append("article");
					break;

				case LaTeXDocumentClass.Report:
					this.Output.Append("report");
					break;

				case LaTeXDocumentClass.Book:
					this.Output.Append("book");
					break;

				case LaTeXDocumentClass.Standalone:
					this.Output.Append("standalone");
					break;
			}
			this.Output.AppendLine("}");

			// Strike-out cf. https://tex.stackexchange.com/questions/546884/strikethrough-command-in-latex
			this.Output.AppendLine("\\newlength{\\wdth}");
			this.Output.AppendLine("\\newcommand{\\strike}[1]{\\settowidth{\\wdth}{#1}\\rlap{\\rule[.5ex]{\\wdth}{.4pt}}#1}");

			if (this.Document.TryGetMetaData("TITLE", out KeyValuePair<string, bool>[] Values))
			{
				MakeTitle = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					this.Output.Append("\\title{");
					this.Output.Append(P.Key);
					this.Output.AppendLine("}");
				}

				if (this.Document.TryGetMetaData("SUBTITLE", out Values))
				{
					foreach (KeyValuePair<string, bool> P2 in Values)
					{
						this.Output.Append("\\subtitle{");
						this.Output.Append(P2.Key);
						this.Output.AppendLine("}");
					}
				}
			}

			if (this.Document.TryGetMetaData("AUTHOR", out Values))
			{
				MakeTitle = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					this.Output.Append("\\author{");
					this.Output.Append(P.Key);
					this.Output.AppendLine("}");
				}
			}

			if (this.Document.TryGetMetaData("DATE", out Values))
			{
				MakeTitle = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					this.Output.Append("\\date{");
					this.Output.Append(P.Key);
					this.Output.AppendLine("}");
				}
			}

			// Todo-lists in LaTeX, cf. https://tex.stackexchange.com/questions/247681/how-to-create-checkbox-todo-list

			this.Output.AppendLine("\\usepackage{enumitem}");
			this.Output.AppendLine("\\usepackage{amssymb}");
			this.Output.AppendLine("\\usepackage{graphicx}");
			this.Output.AppendLine("\\usepackage{pifont}");
			this.Output.AppendLine("\\usepackage{multirow}");
			this.Output.AppendLine("\\usepackage{ragged2e}");
			this.Output.AppendLine("\\newlist{tasklist}{itemize}{2}");
			this.Output.AppendLine("\\setlist[tasklist]{label=$\\square$}");
			this.Output.AppendLine("\\newcommand{\\checkmarksymbol}{\\ding{51}}");
			this.Output.AppendLine("\\newcommand{\\checked}{\\rlap{$\\square$}{\\raisebox{2pt}{\\large\\hspace{1pt}\\checkmarksymbol}}\\hspace{-2.5pt}}");
			this.Output.AppendLine("\\begin{document}");

			if (MakeTitle)
				this.Output.AppendLine("\\maketitle");

			if (this.Document.TryGetMetaData("DESCRIPTION", out Values))
			{
				this.Output.AppendLine("\\begin{abstract}");

				foreach (KeyValuePair<string, bool> P in Values)
					this.Output.AppendLine(EscapeLaTeX(P.Key));

				this.Output.AppendLine("\\end{abstract}");
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
			this.Output.AppendLine("\\end{document}");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Escapes text for output in a LaTeX document.
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <returns>Escaped string.</returns>
		public static string EscapeLaTeX(string s)
		{
			return CommonTypes.Escape(s, latexCharactersToEscape, latexCharacterEscapes);
		}

		private static readonly char[] latexCharactersToEscape = new char[] { '\\', '#', '$', '%', '&', '{', '}' };
		private static readonly string[] latexCharacterEscapes = new string[] { "\\,", "\\#", "\\$", "\\%", "\\&", "\\{", "\\}" };

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
			this.Output.AppendLine(EscapeLaTeX(Element.EMail));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.Output.AppendLine(EscapeLaTeX(Element.URL));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
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
			this.Output.AppendLine(EscapeLaTeX(Element.Emoji.Unicode));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			if (this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false)
			{
				if (Element.AutoExpand)
					await this.Render(Footnote);
				else
				{
					this.Output.Append("\\footnote");

					if (this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false)
					{
						this.Output.Append('[');
						this.Output.Append(Nr.ToString());
						this.Output.Append(']');
					}

					this.Output.Append('{');
					await this.Render(Footnote);
					this.Output.Append('}');
				}
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
			this.Output.Append("\\#");
			this.Output.Append(Element.Tag);

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
				this.Output.Append(EscapeLaTeX(s));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.Output.Append(EscapeLaTeX(new string((char)Element.Code, 1)));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.Output.Append("\\texttt{");
			this.Output.Append(EscapeLaTeX(Element.Code));
			this.Output.Append('}');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
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
				PixelInformation Pixels = G.CreatePixels(out GraphSettings GraphSettings);
				byte[] Bin = Pixels.EncodeAsPng();
				string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\begin{figure}[h]");
					this.Output.AppendLine("\\centering");
				}

				this.Output.Append("\\fbox{\\includegraphics[width=");
				this.Output.Append(((GraphSettings.Width * 3) / 4).ToString());
				this.Output.Append("pt, height=");
				this.Output.Append(((GraphSettings.Height * 3) / 4).ToString());
				this.Output.Append("pt]{");
				this.Output.Append(FileName.Replace('\\', '/'));
				this.Output.Append("}}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\end{figure}");
					this.Output.AppendLine();
				}
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();
				string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\begin{figure}[h]");
					this.Output.AppendLine("\\centering");
				}

				this.Output.Append("\\fbox{\\includegraphics[width=");
				this.Output.Append(((Pixels.Width * 3) / 4).ToString());
				this.Output.Append("pt, height=");
				this.Output.Append(((Pixels.Height * 3) / 4).ToString());
				this.Output.Append("pt]{");
				this.Output.Append(FileName.Replace('\\', '/'));
				this.Output.Append("}}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\end{figure}");
					this.Output.AppendLine();
				}
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();
					string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

					if (AloneInParagraph)
					{
						this.Output.AppendLine("\\begin{figure}[h]");
						this.Output.AppendLine("\\centering");
					}

					this.Output.Append("\\fbox{\\includegraphics[width=");
					this.Output.Append(((Img.Width * 3) / 4).ToString());
					this.Output.Append("pt, height=");
					this.Output.Append(((Img.Height * 3) / 4).ToString());
					this.Output.Append("pt]{");
					this.Output.Append(FileName.Replace('\\', '/'));
					this.Output.Append("}}");

					if (AloneInParagraph)
					{
						this.Output.AppendLine("\\end{figure}");
						this.Output.AppendLine();
					}
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
				bool First = true;

				ex = Log.UnnestException(ex);

				this.Output.AppendLine("\\texttt{\\color{red}");

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						foreach (string Row in ex3.Message.Replace("\r\n", "\n").
							Replace('\r', '\n').Split('\n'))
						{
							if (First)
								First = false;
							else
								this.Output.AppendLine("\\\\");

							this.Output.Append(EscapeLaTeX(Row));
						}
					}
				}
				else
				{
					foreach (string Row in ex.Message.Replace("\r\n", "\n").
						Replace('\r', '\n').Split('\n'))
					{
						if (First)
							First = false;
						else
							this.Output.AppendLine("\\\\");

						this.Output.Append(EscapeLaTeX(Row));
					}
				}

				this.Output.AppendLine("}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine();
					this.Output.AppendLine();
				}
			}
			else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
			{
				this.Output.AppendLine("\\begin{table}[!h]");
				this.Output.AppendLine("\\centering");
				this.Output.Append("\\begin{tabular}{");
				foreach (string _ in M.ColumnNames)
					this.Output.Append("|c");

				this.Output.AppendLine("|}");
				this.Output.AppendLine("\\hline");

				bool First = true;

				foreach (string Name in M.ColumnNames)
				{
					if (First)
						First = false;
					else
						this.Output.Append(" & ");

					this.Output.Append(EscapeLaTeX(Name));
				}

				this.Output.AppendLine("\\\\");
				this.Output.AppendLine("\\hline");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					for (x = 0; x < M.Columns; x++)
					{
						if (x > 0)
							this.Output.Append(" & ");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (!(Item is null))
						{
							if (Item is string s2)
								this.Output.Append(EscapeLaTeX(s2));
							else if (Item is MarkdownElement Element)
							{
								this.Output.Append('{');
								await Element.Render(this);
								this.Output.Append('}');
							}
							else
							{
								this.Output.Append('{');
								this.Output.Append(EscapeLaTeX(Expression.ToString(Item)));
								this.Output.Append('}');
							}
						}

						this.Output.Append("</td>");
					}

					this.Output.AppendLine("\\\\");
				}

				this.Output.AppendLine("\\hline");
				this.Output.AppendLine("\\end{tabular}");
				this.Output.AppendLine("\\end{table}");
				this.Output.AppendLine();
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await this.RenderObject(Item, false, Variables);
			}
			else
				this.Output.Append(EscapeLaTeX(Result?.ToString() ?? string.Empty));

			if (AloneInParagraph)
			{
				this.Output.AppendLine();
				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.Output.Append(EscapeLaTeX(Element.Value));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.Output.AppendLine("\\newline");
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
			bool FirstOnRow = true;

			if (Element.TryGetMetaData(out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						this.Output.Append(' ');

					this.Output.Append(EscapeLaTeX(P.Key));

					if (P.Value)
					{
						this.Output.AppendLine();
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
			IMultimediaLatexRenderer Renderer = Element.MultimediaHandler<IMultimediaLatexRenderer>();
			if (Renderer is null)
				return this.RenderChildren(Element);
			else
				return Renderer.RenderLatex(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
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
				IMultimediaLatexRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaLatexRenderer>();
				if (!(Renderer is null))
					return Renderer.RenderLatex(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.Output.Append("\\textbf{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.Output.Append("\\textsubscript{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.Output.Append("\\textsuperscript{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.Output.Append("\\underline{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			this.Output.AppendLine("\\begin{quote}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{quote}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			this.Output.AppendLine("\\begin{itemize}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{itemize}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			this.Output.AppendLine("\\begin{center}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{center}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			ICodeContentLatexRenderer Renderer = Element.CodeContentHandler<ICodeContentLatexRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					if (await Renderer.RenderLatex(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception ex)
				{
					bool First = true;

					ex = Log.UnnestException(ex);

					this.Output.AppendLine("\\texttt{\\color{red}");

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							foreach (string Row in ex3.Message.Replace("\r\n", "\n").
								Replace('\r', '\n').Split('\n'))
							{
								if (First)
									First = false;
								else
									this.Output.AppendLine("\\\\");

								this.Output.Append(EscapeLaTeX(Row));
							}
						}
					}
					else
					{
						foreach (string Row in ex.Message.Replace("\r\n", "\n").
							Replace('\r', '\n').Split('\n'))
						{
							if (First)
								First = false;
							else
								this.Output.AppendLine("\\\\");

							this.Output.Append(EscapeLaTeX(Row));
						}
					}

					this.Output.AppendLine("}");
					this.Output.AppendLine();
				}
			}

			this.Output.Append("\\texttt{");

			int i;

			for (i = Element.Start; i <= Element.End; i++)
			{
				this.Output.Append(Element.IndentString);
				this.Output.AppendLine(Element.Rows[i]);
			}

			this.Output.AppendLine("}");
			this.Output.AppendLine();
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
		public override async Task Render(DefinitionList Element)
		{
			int State = 0;

			this.Output.AppendLine("\\begin{description}");

			foreach (MarkdownElement E in Element.Children)
			{
				if (E is DefinitionTerms Terms)
				{
					switch (State)
					{
						case 0:
							this.Output.Append("\\item[");
							await Terms.Render(this);
							State++;
							break;

						case 1:
							this.Output.Append(", ");
							await Terms.Render(this);
							break;

						case 2:
							this.Output.AppendLine("}");
							this.Output.Append("\\item[");
							State--;
							await Terms.Render(this);
							break;
					}
				}
				else if (E is DefinitionDescriptions Descriptions)
				{
					switch (State)
					{
						case 0:
							this.Output.Append("\\item{");
							await Descriptions.Render(this);
							State += 2;
							break;

						case 1:
							this.Output.Append("]{");
							await Descriptions.Render(this);
							State++;
							break;

						case 2:
							this.Output.AppendLine();
							await Descriptions.Render(this);
							break;
					}
				}
			}

			switch (State)
			{
				case 1:
					this.Output.AppendLine("]{}");
					break;

				case 2:
					this.Output.AppendLine("}");
					break;
			}

			this.Output.AppendLine("\\end{description}");
			this.Output.AppendLine();
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
		public override async Task Render(DeleteBlocks Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
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
			string Command;

			switch (this.LatexSettings.DocumentClass)
			{
				case LaTeXDocumentClass.Book:
				case LaTeXDocumentClass.Report:
					switch (Element.Level)
					{
						case 1:
							Command = "part";
							break;

						case 2:
							Command = "chapter";
							break;

						case 3:
							Command = "section";
							break;

						case 4:
							Command = "subsection";
							break;

						case 5:
							Command = "subsubsection";
							break;

						case 6:
							Command = "paragraph";
							break;

						case 7:
						default:
							Command = "subparagraph";
							break;
					}
					break;

				case LaTeXDocumentClass.Article:
				case LaTeXDocumentClass.Standalone:
				default:
					switch (Element.Level)
					{
						case 1:
							Command = "section";
							break;

						case 2:
							Command = "subsection";
							break;

						case 3:
							Command = "subsubsection";
							break;

						case 4:
							Command = "paragraph";
							break;

						case 5:
						default:
							Command = "subparagraph";
							break;
					}
					break;
			}

			this.Output.Append('\\');
			this.Output.Append(Command);
			this.Output.Append("*{");

			await this.RenderChildren(Element);

			this.Output.AppendLine("}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.Output.AppendLine("\\hrulefill");
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
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
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
			this.Output.AppendLine("\\begin{flushleft}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{flushleft}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			this.Output.AppendLine("\\begin{justify}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{justify}");
			this.Output.AppendLine();
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
				this.Output.Append("\\item[");
				this.Output.Append(Element.Number.ToString());
				this.Output.Append("]{");
			}
			else
				this.Output.Append("\\item{");

			await this.RenderChild(Element);

			this.Output.AppendLine("}");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			this.Output.AppendLine("\\begin{enumerate}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{enumerate}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			await this.RenderChildren(Element);

			this.Output.AppendLine();
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(RightAligned Element)
		{
			this.Output.AppendLine("\\begin{flushright}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{flushright}");
			this.Output.AppendLine();
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
			this.Output.AppendLine();
			this.Output.AppendLine("\\newpage");
			this.Output.AppendLine();

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
			string s;
			int i, j, k;

			this.Output.AppendLine("\\begin{table}[!h]");
			this.Output.AppendLine("\\centering");
			this.Output.Append("\\begin{tabular}{");
			foreach (TextAlignment Alignment in Element.ColumnAlignments)
			{
				this.Output.Append('|');
				this.RenderAlignment(Alignment);
			}

			this.Output.AppendLine("|}");
			this.Output.AppendLine("\\hline");

			NrRows = Element.Headers.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Headers[RowIndex];
				CellAlignments = Element.HeaderCellAlignments[RowIndex];

				for (i = 0; i < NrColumns; i++)
				{
					if (i > 0)
						this.Output.Append(" & ");

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					if (k > 1)
					{
						this.Output.Append("\\multicolumn{");
						this.Output.Append(k.ToString());
						this.Output.Append("}{|");
						this.RenderAlignment(CellAlignments[i] ?? Element.ColumnAlignments[i]);
						this.Output.Append("|}{");
					}

					E = Row[i];
					if (!(E is null))
						await E.Render(this);

					if (k > 1)
						this.Output.Append('}');
				}

				this.Output.AppendLine("\\\\");
			}

			this.Output.AppendLine("\\hline");

			NrRows = Element.Rows.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Rows[RowIndex];
				CellAlignments = Element.RowCellAlignments[RowIndex];

				for (i = 0; i < NrColumns; i++)
				{
					if (i > 0)
						this.Output.Append(" & ");

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					if (k > 1)
					{
						this.Output.Append("\\multicolumn{");
						this.Output.Append(k.ToString());
						this.Output.Append("}{|");
						this.RenderAlignment(CellAlignments[i] ?? Element.ColumnAlignments[i]);
						this.Output.Append("|}{");
					}

					E = Row[i];
					if (!(E is null))
						await E.Render(this);

					if (k > 1)
						this.Output.Append('}');
				}

				this.Output.AppendLine("\\\\");
			}

			this.Output.AppendLine("\\hline");
			this.Output.AppendLine("\\end{tabular}");

			if (!string.IsNullOrEmpty(Element.Id))
			{
				this.Output.Append("\\caption{");

				s = string.IsNullOrEmpty(Element.Caption) ? Element.Id : Element.Caption;

				this.Output.Append(EscapeLaTeX(s));

				this.Output.AppendLine("}");
				this.Output.Append("\\label{");

				this.Output.Append(EscapeLaTeX(Element.Id));

				this.Output.AppendLine("}");
			}

			this.Output.AppendLine("\\end{table}");
			this.Output.AppendLine();
		}

		private void RenderAlignment(TextAlignment Alignment)
		{
			switch (Alignment)
			{
				case TextAlignment.Left:
				default:
					this.Output.Append('l');
					break;

				case TextAlignment.Center:
					this.Output.Append('c');
					break;

				case TextAlignment.Right:
					this.Output.Append('r');
					break;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.Output.Append("\\item");

			if (Element.IsChecked)
				this.Output.Append("[\\checked]");

			this.Output.Append('{');
			await this.RenderChild(Element);
			this.Output.AppendLine("}");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			this.Output.AppendLine("\\begin{tasklist}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{tasklist}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.Output.Append("\\item{");
			await this.RenderChild(Element);
			this.Output.AppendLine("}");
		}

		#endregion

	}
}
