using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for multimedia content Markdown renderers.
	/// </summary>
	public interface IMultimediaMarkdownRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates Markdown for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderMarkdown(MarkdownRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, 
			MarkdownDocument Document);
	}
}
