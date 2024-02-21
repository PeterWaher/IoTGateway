using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Latex.Multimedia
{
	/// <summary>
	/// Video content.
	/// </summary>
	public class VideoContent : Model.Multimedia.VideoContent, IMultimediaLatexRenderer
	{
		/// <summary>
		/// Video content.
		/// </summary>
		public VideoContent()
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
		public Task RenderLatex(LatexRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;

			foreach (MultimediaItem Item in Items)
				Output.AppendLine(LatexRenderer.EscapeLaTeX(Item.Url));

			return Task.CompletedTask;
		}
	}
}
