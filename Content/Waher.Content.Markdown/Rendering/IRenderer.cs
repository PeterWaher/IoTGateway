using System;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for Markdown renderers.
	/// </summary>
	public interface IRenderer : IDisposable
	{
		/// <summary>
		/// Renders a document.
		/// </summary>
		/// <param name="Document">Document to render.</param>
		/// <param name="Inclusion">If the rendered output is to be included in another document (true), or if it is a standalone document (false).</param>
		Task RenderDocument(MarkdownDocument Document, bool Inclusion);

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Abbreviation Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(AutomaticLinkMail Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(AutomaticLinkUrl Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Delete Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(DetailsReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(EmojiReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Emphasize Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(FootnoteReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(HashTag Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(HtmlEntity Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(HtmlEntityUnicode Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InlineCode Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InlineHTML Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InlineScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InlineText Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Insert Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(LineBreak Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Link Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(LinkReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(MetaReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Model.SpanElements.Multimedia Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(MultimediaReference Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(StrikeThrough Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Strong Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(SubScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(SuperScript Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Underline Element);

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(BlockQuote Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(BulletList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(CenterAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(CodeBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(CommentBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(DefinitionDescriptions Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(DefinitionList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(DefinitionTerms Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(DeleteBlocks Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Footnote Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Header Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(HorizontalRule Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(HtmlBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InsertBlocks Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(InvisibleBreak Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(LeftAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(MarginAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(NestedBlock Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(NumberedItem Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(NumberedList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Paragraph Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(RightAligned Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Sections Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(SectionSeparator Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(Table Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(TaskItem Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(TaskList Element);

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		Task Render(UnnumberedItem Element);

		#endregion
	}
}
