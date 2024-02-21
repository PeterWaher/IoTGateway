using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Contracts
{
	/// <summary>
	/// Interface for multimedia content contract renderers.
	/// </summary>
	public interface IMultimediaContractsRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates smart contract XML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderContractXml(ContractsRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, 
			MarkdownDocument Document);
	}
}
