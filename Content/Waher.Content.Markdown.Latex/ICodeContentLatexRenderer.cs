using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Interface for code content LaTeX renderers.
	/// </summary>
	public interface ICodeContentLatexRenderer : ICodeContentRenderer
	{
		/// <summary>
		/// Generates LaTeX for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		Task<bool> RenderLatex(LatexRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document);
	}
}
