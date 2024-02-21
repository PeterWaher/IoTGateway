using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Abstract base class for Markdown renderers.
	/// </summary>
	public abstract class Renderer : IRenderer
	{
		/// <summary>
		/// Renderer output.
		/// </summary>
		public readonly StringBuilder Output;

		/// <summary>
		/// Reference to Markdown document being processed.
		/// </summary>
		public MarkdownDocument Document;

		/// <summary>
		/// Abstract base class for Markdown renderers.
		/// </summary>
		public Renderer()
			: this(new StringBuilder())
		{
		}

		/// <summary>
		/// Abstract base class for Markdown renderers.
		/// </summary>
		/// <param name="Output">Output.</param>
		public Renderer(StringBuilder Output)
		{
			this.Output = Output;
		}

		/// <summary>
		/// Abstract base class for Markdown renderers.
		/// </summary>
		/// <param name="Document">Document being rendered.</param>
		public Renderer(MarkdownDocument Document)
			: this(new StringBuilder(), Document)
		{
		}

		/// <summary>
		/// Abstract base class for Markdown renderers.
		/// </summary>
		/// <param name="Output">Output.</param>
		/// <param name="Document">Document being rendered.</param>
		public Renderer(StringBuilder Output, MarkdownDocument Document)
		{
			this.Output = Output;
			this.Document = Document;
		}

		/// <summary>
		/// Disposes of the renderer.
		/// </summary>
		public virtual void Dispose()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Renders a document.
		/// </summary>
		/// <param name="Document">Document to render.</param>
		/// <param name="Inclusion">If the rendered output is to be included in another document (true), or if it is a standalone document (false).</param>
		public virtual async Task RenderDocument(MarkdownDocument Document, bool Inclusion)
		{
			MarkdownDocument DocBak = this.Document;

			this.Document = Document;

			if (!Inclusion && this.Document.TryGetMetaData("BODYONLY", out KeyValuePair<string, bool>[] Values))
			{
				if (CommonTypes.TryParse(Values[0].Key, out bool b) && b)
					Inclusion = true;
			}

			if (!Inclusion)
				await this.RenderDocumentHeader();

			foreach (MarkdownElement E in this.Document.Elements)
				await E.Render(this);

			if (this.Document.NeedsToDisplayFootnotes)
				await this.RenderFootnotes();

			if (!Inclusion)
				await this.RenderDocumentFooter();

			this.Document = DocBak;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public virtual Task RenderDocumentHeader()
		{
			return Task.CompletedTask;  // Do nothing by default.
		}

		/// <summary>
		/// Renders footnotes.
		/// </summary>
		public virtual Task RenderFootnotes()
		{
			return Task.CompletedTask;  // Do nothing by default.
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public virtual Task RenderDocumentFooter()
		{
			return Task.CompletedTask;  // Do nothing by default.
		}

		/// <summary>
		/// Returns the renderer output.
		/// </summary>
		/// <returns>Renderer output.</returns>
		public override string ToString()
		{
			return this.Output.ToString();
		}

		/// <summary>
		/// Clears the underlying <see cref="StringBuilder"/>.
		/// </summary>
		public void Clear()
		{
			this.Output.Clear();
		}

		/// <summary>
		/// Renders the children of <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element being rendered</param>
		public async Task RenderChildren(MarkdownElementChildren Element)
		{
			if (!(Element.Children is null))
			{
				foreach (MarkdownElement E in Element.Children)
					await E.Render(this);
			}
		}

		/// <summary>
		/// Renders the children of <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element being rendered</param>
		public async Task RenderChildren(MarkdownElement Element)
		{
			IEnumerable<MarkdownElement> Children = Element.Children;

			if (!(Children is null))
			{
				foreach (MarkdownElement E in Children)
					await E.Render(this);
			}
		}

		/// <summary>
		/// Renders the child of <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element being rendered</param>
		public Task RenderChild(MarkdownElementSingleChild Element)
		{
			if (Element.Child is null)
				return Task.CompletedTask;
			else
				return Element.Child.Render(this);
		}

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Abbreviation Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(AutomaticLinkMail Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(AutomaticLinkUrl Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Delete Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(DetailsReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(EmojiReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Emphasize Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(FootnoteReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(HashTag Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(HtmlEntity Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(HtmlEntityUnicode Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InlineCode Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InlineHTML Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InlineScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InlineText Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Insert Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(LineBreak Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Link Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(LinkReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(MetaReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Model.SpanElements.Multimedia Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(MultimediaReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(StrikeThrough Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Strong Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(SubScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(SuperScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Underline Element);

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(BlockQuote Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(BulletList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(CenterAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(CodeBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(CommentBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(DefinitionDescriptions Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(DefinitionList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(DefinitionTerms Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(DeleteBlocks Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Footnote Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Header Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(HorizontalRule Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(HtmlBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InsertBlocks Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(InvisibleBreak Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(LeftAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(MarginAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(NestedBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(NumberedItem Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(NumberedList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Paragraph Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(RightAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Sections Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(SectionSeparator Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(Table Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(TaskItem Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(TaskList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public abstract Task Render(UnnumberedItem Element);

		#endregion
	}
}
