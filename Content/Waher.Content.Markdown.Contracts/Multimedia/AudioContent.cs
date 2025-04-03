using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;
using Waher.Script;

namespace Waher.Content.Markdown.Contracts.Multimedia
{
	/// <summary>
	/// Audio content.
	/// </summary>
	public class AudioContent : Model.Multimedia.AudioContent, IMultimediaContractsRenderer
	{
		/// <summary>
		/// Audio content.
		/// </summary>
		public AudioContent()
		{
		}

		/// <summary>
		/// Generates LaTeX for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderContractXml(ContractsRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
					await Renderer.RenderObject(Item.Url, true, Variables);
			}
			finally
			{
				Variables?.Pop();
			}
		}
	}
}
