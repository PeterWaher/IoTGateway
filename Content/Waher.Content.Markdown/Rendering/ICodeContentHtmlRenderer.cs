using System.Threading.Tasks;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Interface for code content HTML renderers.
	/// </summary>
	public interface ICodeContentHtmlRenderer : ICodeContentRenderer
	{
		/// <summary>
		/// Generates HTML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		Task<bool> RenderHtml(HtmlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document);
	}
}
