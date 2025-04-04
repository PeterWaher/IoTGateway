using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Functions;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Constants;
using Waher.Script.Graphs;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Renders portable Markdown from a Markdown document.
	/// </summary>
	public class MarkdownRenderer : Renderer
	{
		private bool portableSyntax = true;

		/// <summary>
		/// Renders portable Markdown from a Markdown document.
		/// </summary>
		public MarkdownRenderer()
			: base()
		{
		}

		/// <summary>
		/// Renders portable Markdown from a Markdown document.
		/// </summary>
		/// <param name="Output">Markdown output.</param>
		public MarkdownRenderer(StringBuilder Output)
			: base(Output)
		{
		}

		/// <summary>
		/// Renders portable Markdown from a Markdown document.
		/// </summary>
		/// <param name="Document">Document being rendered.</param>
		public MarkdownRenderer(MarkdownDocument Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Renders portable Markdown from a Markdown document.
		/// </summary>
		/// <param name="Output">Markdown output.</param>
		/// <param name="Document">Document being rendered.</param>
		public MarkdownRenderer(StringBuilder Output, MarkdownDocument Document)
			: base(Output, Document)
		{
		}
		
		/// <summary>
		/// If a portable syntax of markdown is to be generated (true), or if generated
		/// Markdown is to be processed on the same machine (false).
		/// </summary>
		public bool PortableSyntax
		{
			get => this.portableSyntax;
			set => this.portableSyntax = value;
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			if (!(this.Document?.FootnoteOrder is null))
			{
				using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
				{
					foreach (string Key in this.Document.FootnoteOrder)
					{
						if (!Guid.TryParse(Key, out _) &&
							(this.Document?.TryGetFootnote(Key, out Footnote Note) ?? false) &&
							Note.Referenced)
						{
							this.Output.AppendLine();
							this.Output.AppendLine();
							this.Output.Append("[^");
							this.Output.Append(Key);
							this.Output.Append("]:");

							await Note.Render(Renderer);

							string s = Renderer.ToString();
							Renderer.Clear();

							foreach (string Row in s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
							{
								this.Output.Append('\t');
								this.Output.AppendLine(Row);
							}
						}
					}
				}
			}
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Abbreviation Element)
		{
			this.Output.Append('[');
			await this.RenderChildren(Element);
			this.Output.Append("](abbr:");
			this.Output.Append(Element.Description);
			this.Output.Append(')');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkMail Element)
		{
			this.Output.Append('<');
			this.Output.Append(Element.EMail);
			this.Output.Append('>');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.Output.Append('<');
			this.Output.Append(Element.URL);
			this.Output.Append('>');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.Output.Append("~~");
			await this.RenderChildren(Element);
			this.Output.Append("~~");
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
			this.Output.Append(Element.Delimiter);
			this.Output.Append(Element.Emoji.ShortName);
			this.Output.Append(Element.Delimiter);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.Output.Append('*');
			await this.RenderChildren(Element);
			this.Output.Append('*');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			this.Output.Append("[^");

			if (!(this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false))
				Footnote = null;

			if (Guid.TryParse(Element.Key, out _) && !(Footnote is null))
			{
				using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
				{
					await Renderer.Render(Footnote);

					this.Output.Append(Renderer.ToString().TrimEnd());
				}
			}
			else
			{
				this.Output.Append(Element.Key);

				if (!(Footnote is null))
					Footnote.Referenced = true;
			}

			this.Output.Append(']');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
			this.Output.Append('#');
			this.Output.Append(Element.Tag);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
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
			this.Output.Append('`');
			this.Output.Append(Element.Code);
			this.Output.Append('`');

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
		/// Generates Markdown from Script output.
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
				ToMarkdown.GraphToMarkdown(G, this.Output);
				this.Output.AppendLine();
			}
			else if (Result is PixelInformation Pixels)
			{
				ToMarkdown.PixelsToMarkdown(Pixels, this.Output);
				this.Output.AppendLine();
			}
			else if (Result is SKImage Img)
			{
				ToMarkdown.ImageToMarkdown(Img, this.Output);
				this.Output.AppendLine();
			}
			else if (Result is MarkdownDocument Doc)
			{
				using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Output, this.Document))
				{
					await Renderer.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
					Doc.ProcessAsyncTasks();
				}
			}
			else if (Result is MarkdownContent Markdown)
			{
				Doc = await MarkdownDocument.CreateAsync(Markdown.Markdown, Markdown.Settings ?? new MarkdownSettings());
				
				using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Output, this.Document))
				{
					await Renderer.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
					Doc.ProcessAsyncTasks();
				}
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				this.Output.Append("<font class=\"error\">");

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

					this.Output.Append(MarkdownDocument.Encode(ex.Message));

					if (AloneInParagraph)
						this.Output.Append("</p>");
				}

				this.Output.Append("</font>");
			}
			else if (Result is IMatrix M)
			{
				ToMarkdown.MatrixToMarkdown(M, this.Output);
				this.Output.AppendLine();
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

				this.Output.Append(MarkdownDocument.Encode(Result?.ToString() ?? string.Empty));

				if (AloneInParagraph)
					this.Output.Append("</p>");
			}

			if (AloneInParagraph)
				this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.Output.Append(MarkdownDocument.Encode(Element.Value));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.Output.Append("__");
			await this.RenderChildren(Element);
			this.Output.Append("__");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.Output.AppendLine("  ");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Link Element)
		{
			this.Output.Append('[');
			await this.RenderChildren(Element);
			this.Output.Append("](");
			this.Output.Append(Element.Url);

			if (!string.IsNullOrEmpty(Element.Title))
			{
				this.Output.Append(" \"");
				this.Output.Append(Element.Title.Replace("\"", "\\\""));
				this.Output.Append('"');
			}

			this.Output.Append(')');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(LinkReference Element)
		{
			this.Output.Append('[');
			await this.RenderChildren(Element);
			this.Output.Append("][");
			this.Output.Append(Element.Label);
			this.Output.Append(']');
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

					this.Output.Append(MarkdownDocument.Encode(P.Key));
					if (P.Value)
					{
						this.Output.AppendLine("  ");
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
			IMultimediaMarkdownRenderer Renderer = Element.MultimediaHandler<IMultimediaMarkdownRenderer>();
			if (Renderer is null)
				return this.DefaultRenderingMultimedia(Element.Items, Element.Children, Element.AloneInParagraph);
			else
				return Renderer.RenderMarkdown(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
		}

		/// <summary>
		/// Default Multi-media rendering of multi-media content.
		/// </summary>
		/// <param name="Items">Multi-media items</param>
		/// <param name="ChildNodes">Label definition of multi-media content.</param>
		/// <param name="AloneInParagraph">If the multi-media construct is alone in the paragraph.</param>
		private async Task DefaultRenderingMultimedia(MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph)
		{
			bool First = true;

			this.Output.Append("![");
			await this.Render(ChildNodes);
			this.Output.Append(']');

			foreach (MultimediaItem Item in Items)
			{
				if (First)
					First = false;
				else if (AloneInParagraph)
				{
					this.Output.AppendLine();
					this.Output.Append('\t');
				}

				this.Output.Append('(');
				this.Output.Append(Item.Url);

				if (!string.IsNullOrEmpty(Item.Title))
				{
					this.Output.Append(" \"");
					this.Output.Append(Item.Title.Replace("\"", "\\\""));
					this.Output.Append('"');
				}

				if (Item.Width.HasValue)
				{
					this.Output.Append(' ');
					this.Output.Append(Item.Width.Value.ToString());

					if (Item.Height.HasValue)
					{
						this.Output.Append(' ');
						this.Output.Append(Item.Height.Value.ToString());
					}
				}

				this.Output.Append(')');
			}

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
		public override Task Render(MultimediaReference Element)
		{
			Model.SpanElements.Multimedia Multimedia = Element.Document.GetReference(Element.Label);

			if (!(Multimedia is null))
			{
				IMultimediaMarkdownRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaMarkdownRenderer>();
				if (Renderer is null)
					return this.DefaultRenderingMultimedia(Multimedia.Items, Element.Children, Element.AloneInParagraph);
				else
					return Renderer.RenderMarkdown(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.Output.Append('~');
			await this.RenderChildren(Element);
			this.Output.Append('~');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.Output.Append("**");
			await this.RenderChildren(Element);
			this.Output.Append("**");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.Output.Append('[');
			await this.RenderChildren(Element);
			this.Output.Append(']');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.Output.Append("^[");
			await this.RenderChildren(Element);
			this.Output.Append(']');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.Output.Append('_');
			await this.RenderChildren(Element);
			this.Output.Append('_');
		}

		#endregion

		#region Indentation

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="Prefix">Block prefix</param>
		private Task PrefixedBlock(ChunkedList<MarkdownElement> Children, string Prefix)
		{
			return this.PrefixedBlock(Children, Prefix, Prefix);
		}

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Child">Child element.</param>
		/// <param name="PrefixFirstRow">Prefix, for first row.</param>
		/// <param name="PrefixNextRows">Prefix, for the rest of the rows, if any.</param>
		private Task PrefixedBlock(MarkdownElement Child, string PrefixFirstRow, string PrefixNextRows)
		{
			return this.PrefixedBlock(new ChunkedList<MarkdownElement>(Child), PrefixFirstRow, PrefixNextRows);
		}

		/// <summary>
		/// Prefixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="PrefixFirstRow">Prefix, for first row.</param>
		/// <param name="PrefixNextRows">Prefix, for the rest of the rows, if any.</param>
		private async Task PrefixedBlock(ChunkedList<MarkdownElement> Children, string PrefixFirstRow, string PrefixNextRows)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
			{
				await Renderer.Render(Children);

				string s = Renderer.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
				string[] Rows = s.Split('\n');
				int i, c = Rows.Length;

				if (c > 0 && string.IsNullOrEmpty(Rows[c - 1]))
					c--;

				for (i = 0; i < c; i++)
				{
					this.Output.Append(PrefixFirstRow);
					this.Output.AppendLine(Rows[i]);
					PrefixFirstRow = PrefixNextRows;
				}
			}
		}

		/// <summary>
		/// Suffixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="Suffix">Block suffix</param>
		private Task SuffixedBlock(ChunkedList<MarkdownElement> Children, string Suffix)
		{
			return this.SuffixedBlock(Children, Suffix, Suffix);
		}

		/// <summary>
		/// Suffixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="SuffixFirstRow">Suffix, for first row.</param>
		/// <param name="SuffixNextRows">Suffix, for the rest of the rows, if any.</param>
		private async Task SuffixedBlock(ChunkedList<MarkdownElement> Children, string SuffixFirstRow, string SuffixNextRows)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
			{
				await Renderer.Render(Children);

				string s = Renderer.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
				string[] Rows = s.Split('\n');
				int i, c = Rows.Length;

				if (c > 0 && string.IsNullOrEmpty(Rows[c - 1]))
					c--;

				for (i = 0; i < c; i++)
				{
					this.Output.Append(Rows[i]);
					this.Output.AppendLine(SuffixFirstRow);
					SuffixFirstRow = SuffixNextRows;
				}
			}
		}

		/// <summary>
		/// Prefixes and Suffixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="Prefix">Block prefix</param>
		/// <param name="Suffix">Block suffix</param>
		private Task PrefixSuffixedBlock(ChunkedList<MarkdownElement> Children, string Prefix, string Suffix)
		{
			return this.PrefixSuffixedBlock(Children, Prefix, Prefix, Suffix, Suffix);
		}

		/// <summary>
		/// Prefixes and Suffixes a block of markdown.
		/// </summary>
		/// <param name="Children">Child elements.</param>
		/// <param name="PrefixFirstRow">Prefix, for first row.</param>
		/// <param name="PrefixNextRows">Prefix, for the rest of the rows, if any.</param>
		/// <param name="SuffixFirstRow">Suffix, for first row.</param>
		/// <param name="SuffixNextRows">Suffix, for the rest of the rows, if any.</param>
		private async Task PrefixSuffixedBlock(ChunkedList<MarkdownElement> Children, string PrefixFirstRow, string PrefixNextRows,
			string SuffixFirstRow, string SuffixNextRows)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
			{
				await Renderer.Render(Children);

				string s = Renderer.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
				string[] Rows = s.Split('\n');
				int i, c = Rows.Length;

				if (c > 0 && string.IsNullOrEmpty(Rows[c - 1]))
					c--;

				for (i = 0; i < c; i++)
				{
					this.Output.Append(PrefixFirstRow);
					this.Output.Append(Rows[i]);
					this.Output.AppendLine(SuffixFirstRow);
					PrefixFirstRow = PrefixNextRows;
					SuffixFirstRow = SuffixNextRows;
				}
			}
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			await this.PrefixedBlock(Element.Children, ">\t");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			await this.RenderChildren(Element);
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			await this.PrefixSuffixedBlock(Element.Children, ">>", "<<");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			if (this.portableSyntax)
			{
				ICodeContentMarkdownRenderer Renderer = Element.CodeContentHandler<ICodeContentMarkdownRenderer>();

				if (!(Renderer is null))
				{
					try
					{
						if (await Renderer.RenderMarkdown(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
							return;
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			int Max = 2;

			foreach (string Row in Element.Rows)
			{
				int i = 0;

				foreach (char ch in Row)
				{
					if (ch == '`')
						i++;
					else
					{
						if (i > Max)
							Max = i;

						i = 0;
					}
				}

				if (i > Max)
					Max = i;
			}

			string s = new string('`', Max + 1);

			this.Output.Append(s);
			this.Output.AppendLine(Element.Language);

			foreach (string Row in Element.Rows)
				this.Output.AppendLine(Row);

			this.Output.AppendLine(s);
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
		public override async Task Render(DefinitionDescriptions Element)
		{
			await this.PrefixedBlock(Element.Children, ":\t", ":\t");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionList Element)
		{
			await this.RenderChildren(Element);
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionTerms Element)
		{
			ChunkNode<MarkdownElement> Loop = Element.Children.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					await Loop[i].Render(this);
					this.Output.AppendLine();
				}

				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			await this.PrefixedBlock(Element.Children, "->\t");
			this.Output.AppendLine();
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
			if (Element.Prefix)
			{
				this.Output.Append(Element.Row);
				this.Output.Append(' ');
			}

			await this.RenderChildren(Element);
			this.Output.AppendLine();

			if (!Element.Prefix)
				this.Output.AppendLine(Element.Row);

			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.Output.AppendLine(Element.Row);
			this.Output.AppendLine();

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
			await this.PrefixedBlock(Element.Children, "+>\t");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InvisibleBreak Element)
		{
			this.Output.AppendLine(Element.Row);
			this.Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(LeftAligned Element)
		{
			await this.PrefixedBlock(Element.Children, "<<");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			await this.PrefixSuffixedBlock(Element.Children, "<<", ">>");
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
			string Prefix;

			if (Element.NumberExplicit)
				Prefix = Element.Number.ToString() + ".\t";
			else
				Prefix = "#.\t";

			await this.PrefixedBlock(Element.Child, Prefix, "\t");

			if (Element.Child.IsBlockElement)
				this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			await this.RenderChildren(Element);
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
			await this.SuffixedBlock(Element.Children, ">>");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Sections Element)
		{
			if (!string.IsNullOrEmpty(Element.InitialRow))
			{
				this.Output.AppendLine(Element.InitialRow);
				this.Output.AppendLine();
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.Output.AppendLine(Element.Row);
			this.Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			int[] Widths = new int[Element.Columns];
			int i, c, d;

			string[][] Headers = new string[c = Element.Headers.Length][];
			for (i = 0; i < c; i++)
				Headers[i] = await this.Render(Element.Headers[i], Element.HeaderCellAlignments[i], Widths, Element);

			string[][] Rows = new string[d = Element.Rows.Length][];
			for (i = 0; i < d; i++)
				Rows[i] = await this.Render(Element.Rows[i], Element.RowCellAlignments[i], Widths, Element);

			for (i = 0; i < c; i++)
				this.Render(Headers[i], Widths, Element);

			foreach (string Headline in Element.ColumnAlignmentDefinitions)
			{
				this.Output.Append('|');
				this.Output.Append(Headline);
			}

			this.Output.AppendLine("|");

			for (i = 0; i < d; i++)
				this.Render(Rows[i], Widths, Element);

			bool NewLine = false;

			if (!string.IsNullOrEmpty(Element.Caption))
			{
				this.Output.Append('[');
				this.Output.Append(Element.Caption);
				this.Output.Append(']');
				NewLine = true;
			}

			if (!string.IsNullOrEmpty(Element.Id))
			{
				this.Output.Append('[');
				this.Output.Append(Element.Id);
				this.Output.Append(']');
				NewLine = true;
			}

			if (NewLine)
				this.Output.AppendLine();

			this.Output.AppendLine();
		}

		private void Render(string[] Elements, int[] Widths, Table Element)
		{
			string s;
			int i, j, k;

			this.Output.Append('|');

			for (i = 0; i < Element.Columns;)
			{
				s = Elements[i];
				if (s is null)
					continue;

				this.Output.Append(' ');
				this.Output.Append(s);
				j = Widths[i] - s.Length;
				k = 1;

				i++;
				while (i < Element.Columns && Elements[i] is null)
				{
					j += Widths[i++];
					k++;
				}

				while (j-- > 0)
					this.Output.Append(' ');

				while (k-- > 0)
					this.Output.Append('|');
			}

			this.Output.AppendLine();
		}

		private async Task<string[]> Render(MarkdownElement[] Elements, TextAlignment?[] Alignments, int[] Widths, Table Element)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(this.Document))
			{
				string[] Result = new string[Element.Columns];
				string s;
				MarkdownElement E;
				TextAlignment? Alignment;
				int Len, LastLen;
				int i, j;

				for (i = 0; i < Element.Columns; i++)
				{
					E = Elements[i];
					if (E is null)
						continue;

					await E.Render(Renderer);
					s = Renderer.ToString();
					Renderer.Clear();

					Alignment = Alignments[i];
					if (Alignment.HasValue)
					{
						switch (Alignment.Value)
						{
							case TextAlignment.Left:
								s = "<<" + s;
								break;

							case TextAlignment.Right:
								s += ">>";
								break;

							case TextAlignment.Center:
								s = ">>" + s + "<<";
								break;
						}
					}

					Result[i] = s;

					LastLen = s.Length + 2;    // One space on each side of content.
					j = i + 1;
					while (j < Element.Columns && Elements[j] is null)
					{
						Result[j++] = null;
						LastLen++;              // One additional pipe character
					}

					j -= i;

					Len = LastLen / j;
					LastLen -= (j - 1) * Len;

					while (j-- > 1)
					{
						if (Widths[i] < Len)
							Widths[i] = Len;

						i++;
					}

					if (Widths[i] < LastLen)
						Widths[i] = LastLen;
				}

				return Result;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			await this.PrefixedBlock(Element.Child, Element.IsChecked ? "[x]\t" : "[ ]\t", "\t");

			if (Element.Child.IsBlockElement)
				this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			await this.RenderChildren(Element);
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			await this.PrefixedBlock(Element.Child, Element.Prefix + "\t", "\t");

			if (Element.Child.IsBlockElement)
				this.Output.AppendLine();
		}

		#endregion

	}
}
