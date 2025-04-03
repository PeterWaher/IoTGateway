using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
    /// <summary>
    /// Markdown content.
    /// </summary>
    public class MarkdownContent : Model.Multimedia.MarkdownContent, IMultimediaXamarinFormsXamlRenderer
    {
        /// <summary>
        /// Markdown content.
        /// </summary>
        public MarkdownContent()
        {
        }

		/// <summary>
		/// Generates Xamarin.Forms XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			return ProcessInclusion(Renderer, Items, Document);
		}
	}
}
