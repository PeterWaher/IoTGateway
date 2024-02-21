using System.Threading.Tasks;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for code content Markdown renderers.
	/// </summary>
	public interface ICodeContentMarkdownRenderer : ICodeContentRenderer
	{
		/// <summary>
		/// Generates Markdown for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		Task<bool> RenderMarkdown(MarkdownRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document);
	}
}
