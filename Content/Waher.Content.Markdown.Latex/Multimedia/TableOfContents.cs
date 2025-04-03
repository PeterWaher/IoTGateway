using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Latex.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : Model.Multimedia.TableOfContents, IMultimediaLatexRenderer
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}
		/// <summary>
		/// Generates LaTeX for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderLatex(LatexRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Renderer.Output.AppendLine("\\tableofcontents");
			return Task.CompletedTask;
		}
	}
}
