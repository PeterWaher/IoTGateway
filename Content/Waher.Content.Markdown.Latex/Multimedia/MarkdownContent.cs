using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Latex.Multimedia
{
    /// <summary>
    /// Markdown content.
    /// </summary>
    public class MarkdownContent : Model.Multimedia.MarkdownContent, IMultimediaLatexRenderer
    {
        /// <summary>
        /// Markdown content.
        /// </summary>
        public MarkdownContent()
        {
        }

		/// <summary>
		/// Generates smart contract XML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderLatex(LatexRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			await ProcessInclusion(Renderer, Items, Document);

			if (AloneInParagraph)
				Renderer.Output.AppendLine();
		}

	}
}
