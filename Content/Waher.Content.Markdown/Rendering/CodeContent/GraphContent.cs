using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Rendering.CodeContent
{
	/// <summary>
	/// Script graph content.
	/// </summary>
	public class GraphContent : Model.CodeContent.GraphContent, ICodeContentHtmlRenderer, ICodeContentTextRenderer
	{
		/// <summary>
		/// Script graph content.
		/// </summary>
		public GraphContent()
		{
		}

		/// <summary>
		/// Generates HTML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderHtml(HtmlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);
				await Renderer.RenderObject(G, true, Document.Settings.Variables ?? new Variables());

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Generates plain text for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderText(TextRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);
				await Renderer.RenderObject(G, true);
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}
	}
}
