using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Markdown rendering extensions for LaTeX.
	/// </summary>
	public static class LatexExtensions
	{
		/// <summary>
		/// Generates LaTeX from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <returns>LaTeX</returns>
		public static async Task<string> GenerateLaTeX(this MarkdownDocument Document)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateLaTeX(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates LaTeX from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">LaTeX will be output here.</param>
		public static Task GenerateLaTeX(this MarkdownDocument Document, StringBuilder Output)
		{
			return Document.GenerateLaTeX(Output, new LaTeXSettings());
		}

		/// <summary>
		/// Generates LaTeX from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <returns>LaTeX</returns>
		public static async Task<string> GenerateLaTeX(this MarkdownDocument Document, LaTeXSettings LaTeXSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateLaTeX(Output, LaTeXSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates LaTeX from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="LaTeXSettings">LaTeX settings.</param>
		public static async Task GenerateLaTeX(this MarkdownDocument Document, StringBuilder Output, LaTeXSettings LaTeXSettings)
		{
			using (LatexRenderer Renderer = new LatexRenderer(Output, LaTeXSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}
	}
}