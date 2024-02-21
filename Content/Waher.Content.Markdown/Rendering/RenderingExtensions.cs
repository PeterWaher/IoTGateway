using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Rendering
{
	/// <summary>
	/// Rendering extensions for some backward-compatibility.
	/// </summary>
	public static class MarkdownRenderingExtensions
	{
		/// <summary>
		/// Renders Markdown from a Markdown element.
		/// </summary>
		/// <param name="Element">Markdown element to render.</param>
		/// <param name="Output">Output.</param>
		public static async Task GenerateMarkdown(this MarkdownElement Element, StringBuilder Output)
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer(Output))
			{
				await Element.Render(Renderer);
			}
		}

		/// <summary>
		/// Renders HTML from a Markdown element.
		/// </summary>
		/// <param name="Element">Markdown element to render.</param>
		/// <param name="Output">Output.</param>
		public static async Task GenerateHTML(this MarkdownElement Element, StringBuilder Output)
		{
			using (HtmlRenderer Renderer = new HtmlRenderer(Output, new HtmlSettings()))
			{
				await Element.Render(Renderer);
			}
		}

		/// <summary>
		/// Renders plain text from a Markdown element.
		/// </summary>
		/// <param name="Element">Markdown element to render.</param>
		/// <param name="Output">Output.</param>
		public static async Task GenerateText(this MarkdownElement Element, StringBuilder Output)
		{
			using (TextRenderer Renderer = new TextRenderer(Output))
			{
				await Element.Render(Renderer);
			}
		}
	}
}
