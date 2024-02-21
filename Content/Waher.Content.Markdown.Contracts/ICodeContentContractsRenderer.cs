using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Contracts
{
	/// <summary>
	/// Interface for code content contract renderers.
	/// </summary>
	public interface ICodeContentContractsRenderer : ICodeContentRenderer
	{
		/// <summary>
		/// Generates smart contract XML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		Task<bool> RenderContractXml(ContractsRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document);
	}
}
