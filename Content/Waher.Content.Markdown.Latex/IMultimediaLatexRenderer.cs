using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Interface for multimedia content LaTeX renderers.
	/// </summary>
	public interface IMultimediaLatexRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates LaTeX for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderLatex(LatexRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, bool AloneInParagraph, 
			MarkdownDocument Document);
	}
}
