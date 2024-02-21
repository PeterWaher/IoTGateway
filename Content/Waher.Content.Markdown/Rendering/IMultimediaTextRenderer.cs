using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for multimedia content plain text renderers.
	/// </summary>
	public interface IMultimediaTextRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates plain text for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderText(TextRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, 
			MarkdownDocument Document);
	}
}
