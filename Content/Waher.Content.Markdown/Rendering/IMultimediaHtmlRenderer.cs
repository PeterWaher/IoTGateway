using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for multimedia content HTML renderers.
	/// </summary>
	public interface IMultimediaHtmlRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates HTML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, bool AloneInParagraph, 
			MarkdownDocument Document);

		/// <summary>
		/// Reports a resource for preloading.
		/// </summary>
		/// <param name="Progress">Progress reporting interface.</param>
		/// <param name="Items">Multi-media items.</param>
		Task Preload(ICodecProgress Progress, MultimediaItem[] Items);
	}
}
