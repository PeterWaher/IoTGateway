using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Wpf.CodeContent
{
	/// <summary>
	/// Script graph content.
	/// </summary>
	public class GraphContent : Model.CodeContent.GraphContent, ICodeContentWpfXamlRenderer
	{
		/// <summary>
		/// Script graph content.
		/// </summary>
		public GraphContent()
		{
		}

		/// <summary>
		/// Generates WPF XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderWpfXaml(WpfXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
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
	}
}
