using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
    /// <summary>
    /// Markdown content.
    /// </summary>
    public class MarkdownContent : Model.Multimedia.MarkdownContent, IMultimediaHtmlRenderer, IMultimediaTextRenderer, IMultimediaMarkdownRenderer
    {
        /// <summary>
        /// Markdown content.
        /// </summary>
        public MarkdownContent()
        {
        }

		/// <summary>
		/// Generates HTML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
			await ProcessInclusion(Renderer, Items, Document);

			if (AloneInParagraph)
				Renderer.Output.AppendLine();
        }

		/// <summary>
		/// Generates plain text for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderText(TextRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
			await ProcessInclusion(Renderer, Items, Document);

			if (AloneInParagraph)
				Renderer.Output.AppendLine();
		}

		/// <summary>
		/// Generates Markdown for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderMarkdown(MarkdownRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			await ProcessInclusion(Renderer, Items, Document);

			if (AloneInParagraph)
				Renderer.Output.AppendLine();
		}

		/// <summary>
		/// Reports a resource for preloading.
		/// </summary>
		/// <param name="Progress">Progress reporting interface.</param>
		/// <param name="Items">Multi-media items.</param>
		public Task Preload(ICodecProgress Progress, MultimediaItem[] Items)
		{
			return Task.CompletedTask;
		}
	}
}
