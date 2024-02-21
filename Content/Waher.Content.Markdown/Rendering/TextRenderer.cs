using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Events;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Renders plain text from a Markdown document.
	/// </summary>
	public class TextRenderer : Renderer
	{
		/// <summary>
		/// Renders plain text from a Markdown document.
		/// </summary>
		public TextRenderer()
			: base()
		{
		}

		/// <summary>
		/// Renders plain text from a Markdown document.
		/// </summary>
		/// <param name="Output">Plain text output.</param>
		public TextRenderer(StringBuilder Output)
			: base(Output)
		{
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public override async Task RenderFootnotes()
		{
			this.Output.AppendLine(new string('-', 80));
			this.Output.AppendLine();

			foreach (string Key in this.Document.FootnoteOrder)
			{
				if ((this.Document?.TryGetFootnote(Key, out Footnote Note) ?? false) &&
					Note.Referenced)
				{
					this.Output.Append('[');

					if (this.Document?.TryGetFootnoteNumber(Key, out int Nr) ?? false)
						this.Output.Append(Nr.ToString());
					else
						this.Output.Append(Key);

					this.Output.Append("] ");

					await Note.Render(this);
				}
			}
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
			this.Output.Append(Element.EMail);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.Output.Append(Element.URL);

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
			this.Output.Append(Element.Emoji.Unicode);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Emphasize Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(FootnoteReference Element)
		{
			if (this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false)
				Footnote.Referenced = true;

			this.Output.Append(" [");

			if ((this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false))
				this.Output.Append(Nr.ToString());
			else
				this.Output.Append(Element.Key);

			this.Output.Append(']');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
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
				this.Output.Append(s);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.Output.Append((char)Element.Code);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.Output.Append(Element.Code);

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
			await this.RenderObject(Result, Element.AloneInParagraph);
		}

		/// <summary>
		/// Generates plain text from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		public Task RenderObject(object Result, bool AloneInParagraph)
		{
			if (Result is null)
				return Task.CompletedTask;

			this.Output.Append(Result.ToString());

			if (AloneInParagraph)
			{
				this.Output.AppendLine();
				this.Output.AppendLine();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.Output.Append(Element.Value);

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
			this.Output.AppendLine();

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

					this.Output.Append(P.Key);
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
			IMultimediaTextRenderer Renderer = Element.MultimediaHandler<IMultimediaTextRenderer>();
			if (Renderer is null)
				return this.DefaultRenderingMultimedia(Element.Children, Element.AloneInParagraph);
			else
				return Renderer.RenderText(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
		}

		/// <summary>
		/// Default Multi-media rendering of multi-media content.
		/// </summary>
		/// <param name="ChildNodes">Label definition of multi-media content.</param>
		/// <param name="AloneInParagraph">If the multi-media construct is alone in the paragraph.</param>
		private async Task DefaultRenderingMultimedia(IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph)
		{
			foreach (MarkdownElement E in ChildNodes)
				await E.Render(this);

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
				IMultimediaTextRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaTextRenderer>();
				if (Renderer is null)
					return this.DefaultRenderingMultimedia(Element.Children, Element.AloneInParagraph);
				else
					return Renderer.RenderText(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(StrikeThrough Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Strong Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			int i;

			using (TextRenderer Renderer = new TextRenderer())
			{
				await Renderer.RenderChildren(Element);

				foreach (char ch in Renderer.ToString())
				{
					i = nrmScriptLetters.IndexOf(ch);
					if (i < 0)
						this.Output.Append(ch);
					else
						this.Output.Append(subScriptLetters[i]);
				}
			}
		}

		private const string nrmScriptLetters = "0123456789+-=()aeoxhklmnpstijruv";
		private const string subScriptLetters = "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎ₐₑₒₓₕₖₗₘₙₚₛₜᵢⱼᵣᵤᵥ";

		/// <summary>
		/// Converts a string to subscript (as far as it goes).
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String with subscript characters.</returns>
		public static string ToSubscript(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			foreach (char ch in s)
			{
				i = nrmScriptLetters.IndexOf(ch);
				if (i < 0)
					sb.Append(ch);
				else
					sb.Append(subScriptLetters[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			int i;

			using (TextRenderer Renderer = new TextRenderer())
			{
				await Renderer.RenderChildren(Element);

				foreach (char ch in Renderer.ToString())
				{
					i = normlScriptLetters.IndexOf(ch);
					if (i < 0)
						this.Output.Append(ch);
					else
						this.Output.Append(superScriptLetters[i]);
				}
			}
		}

		private const string normlScriptLetters = "abcdefghijklmnoprstuvwxyzABDEGHIJKLMNOPRTUW0123456789+-=()";
		private const string superScriptLetters = "ᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷˣʸᶻᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂ⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";

		/// <summary>
		/// Converts a string to superscript (as far as it goes).
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String with superscript characters.</returns>
		public static string ToSuperscript(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i;

			foreach (char ch in s)
			{
				i = normlScriptLetters.IndexOf(ch);
				if (i < 0)
					sb.Append(ch);
				else
					sb.Append(superScriptLetters[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Underline Element)
		{
			return this.RenderChildren(Element);
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				await Renderer.RenderChildren(Element);

				string s = Renderer.ToString().Trim();
				s = s.Replace("\r\n", "\n").Replace("\r", "\n");

				foreach (string Row in s.Split('\n'))
				{
					this.Output.Append("> ");
					this.Output.AppendLine(Row);
				}

				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;
				string s2 = Environment.NewLine + Environment.NewLine;
				bool LastIsParagraph = false;

				s = this.Output.ToString();
				if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
					this.Output.AppendLine();

				foreach (MarkdownElement E in Element.Children)
				{
					await E.Render(Renderer);

					s = Renderer.ToString();
					Renderer.Clear();
					this.Output.Append(s);

					LastIsParagraph = s.EndsWith(s2);
				}

				if (!LastIsParagraph)
					this.Output.AppendLine();
			}
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
			ICodeContentTextRenderer Renderer = Element.CodeContentHandler<ICodeContentTextRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					if (await Renderer.RenderText(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
							this.Output.AppendLine(ex3.Message);
					}
					else
						this.Output.AppendLine(ex.Message);
				}
			}
			else
			{
				int i;

				for (i = Element.Start; i <= Element.End; i++)
				{
					this.Output.Append(Element.IndentString);
					this.Output.AppendLine(Element.Rows[i]);
				}

				this.Output.AppendLine();
			}
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
				this.Output.Append(":\t");
				await E.Render(this);
				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionList Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;
				string s2 = Environment.NewLine + Environment.NewLine;
				bool LastIsParagraph = false;
				bool FirstTerm = true;

				s = this.Output.ToString();
				if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
					this.Output.AppendLine();

				foreach (MarkdownElement E in Element.Children)
				{
					if (E is DefinitionTerms)
					{
						if (FirstTerm)
							FirstTerm = false;
						else
							this.Output.AppendLine();
					}

					await E.Render(Renderer);
					s = Renderer.ToString();
					Renderer.Clear();
					this.Output.Append(s);

					LastIsParagraph = s.EndsWith(s2);
				}

				if (!LastIsParagraph)
					this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionTerms Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;

				foreach (MarkdownElement E in Element.Children)
				{
					await E.Render(Renderer);
					s = Renderer.ToString();
					Renderer.Clear();
					this.Output.Append(s);

					if (!s.EndsWith(Environment.NewLine))
						this.Output.AppendLine();
				}
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				foreach (MarkdownElement E in Element.Children)
					await E.Render(Renderer);

				string s = Renderer.ToString().Trim();
				s = s.Replace("\r\n", "\n").Replace("\r", "\n");

				foreach (string Row in s.Split('\n'))
				{
					this.Output.Append("-> ");
					this.Output.AppendLine(Row);
				}

				this.Output.AppendLine();
			}
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
			if (Element.Level <= 2)
			{
				int Len = this.Output.Length;

				await this.RenderChildren(Element);

				Len = this.Output.Length - Len + 3;
				this.Output.AppendLine();
				this.Output.AppendLine(new string(Element.Level == 1 ? '=' : '-', Len));
				this.Output.AppendLine();
			}
			else
			{
				this.Output.Append(new string('#', Element.Level));
				this.Output.Append(' ');

				await this.RenderChildren(Element);

				this.Output.AppendLine();
				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.Output.AppendLine(new string('-', 80));
			this.Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;
				bool First = true;

				foreach (MarkdownElement E in Element.Children)
				{
					await E.Render(Renderer);
					s = Renderer.ToString().TrimStart().Trim(' ');    // Only space at the end, not CRLF
					Renderer.Clear();

					if (!string.IsNullOrEmpty(s))
					{
						if (First)
							First = false;
						else
							this.Output.Append(' ');

						this.Output.Append(s);

						if (s.EndsWith("\n"))
							First = true;
					}
				}

				this.Output.AppendLine();
				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				foreach (MarkdownElement E in Element.Children)
					await E.Render(Renderer);

				string s = Renderer.ToString().Trim();
				s = s.Replace("\r\n", "\n").Replace("\r", "\n");

				foreach (string Row in s.Split('\n'))
				{
					this.Output.Append("+> ");
					this.Output.AppendLine(Row);
				}

				this.Output.AppendLine();
			}
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
			this.Output.Append(Element.Number.ToString());
			this.Output.Append(". ");

			using (TextRenderer Renderer = new TextRenderer())
			{
				await Renderer.RenderChild(Element);

				string s = Renderer.ToString();

				this.Output.Append(s);

				if (!s.EndsWith("\n"))
					this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;
				string s2 = Environment.NewLine + Environment.NewLine;
				bool LastIsParagraph = false;

				s = this.Output.ToString();
				if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
					this.Output.AppendLine();

				foreach (MarkdownElement E in Element.Children)
				{
					await E.Render(Renderer);
					s = Renderer.ToString();
					Renderer.Clear();
					this.Output.Append(s);

					LastIsParagraph = s.EndsWith(s2);
				}

				if (!LastIsParagraph)
					this.Output.AppendLine();
			}
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
			this.Output.AppendLine(new string('=', 80));
			this.Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			bool First;

			foreach (MarkdownElement[] Row in Element.Headers)
			{
				First = true;

				foreach (MarkdownElement E in Row)
				{
					if (E is null)
						continue;

					if (First)
						First = false;
					else
						this.Output.Append('\t');

					await E.Render(this);
				}

				this.Output.AppendLine();
			}

			foreach (MarkdownElement[] Row in Element.Rows)
			{
				First = true;

				foreach (MarkdownElement E in Row)
				{
					if (E is null)
						continue;

					if (First)
						First = false;
					else
						this.Output.Append('\t');

					await E.Render(this);
				}

				this.Output.AppendLine();
			}

			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			if (Element.IsChecked)
				this.Output.Append("[X] ");
			else
				this.Output.Append("[ ] ");

			using (TextRenderer Renderer = new TextRenderer())
			{
				await Element.Child.Render(Renderer);

				string s = Renderer.ToString();

				this.Output.Append(s);

				if (!s.EndsWith("\n"))
					this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				string s;
				string s2 = Environment.NewLine + Environment.NewLine;
				bool LastIsParagraph = false;

				s = this.Output.ToString();
				if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
					this.Output.AppendLine();

				foreach (MarkdownElement E in Element.Children)
				{
					await E.Render(Renderer);
					s = Renderer.ToString();
					Renderer.Clear();
					this.Output.Append(s);

					LastIsParagraph = s.EndsWith(s2);
				}

				if (!LastIsParagraph)
					this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			using (TextRenderer Renderer = new TextRenderer())
			{
				this.Output.Append(Element.Prefix);
				this.Output.Append(' ');

				await Element.Child.Render(Renderer);

				string s = Renderer.ToString();

				this.Output.Append(s);

				if (!s.EndsWith("\n"))
					this.Output.AppendLine();
			}
		}

		#endregion

	}
}
