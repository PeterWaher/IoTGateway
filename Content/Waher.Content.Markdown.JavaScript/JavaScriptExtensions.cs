using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.JavaScript
{
	/// <summary>
	/// Markdown rendering extensions for JavaScript.
	/// </summary>
	public static class JavaScriptExtensions
	{
		/// <summary>
		/// Generates JavaScript from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <returns>JavaScript</returns>
		public static async Task<string> GenerateJavaScript(this MarkdownDocument Document)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateJavaScript(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Generates JavaScript from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">JavaScript will be output here.</param>
		public static Task GenerateJavaScript(this MarkdownDocument Document, StringBuilder Output)
		{
			return Document.GenerateJavaScript(Output, new HtmlSettings());
		}

		/// <summary>
		/// Generates JavaScript from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="HtmlSettings">JavaScript settings.</param>
		/// <returns>JavaScript</returns>
		public static async Task<string> GenerateJavaScript(this MarkdownDocument Document, HtmlSettings HtmlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateJavaScript(Output, HtmlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates JavaScript from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">JavaScript will be output here.</param>
		/// <param name="HtmlSettings">JavaScript settings.</param>
		public static async Task GenerateJavaScript(this MarkdownDocument Document, StringBuilder Output, HtmlSettings HtmlSettings)
		{
			using (JavaScriptRenderer Renderer = new JavaScriptRenderer(Output, HtmlSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}
	}
}