using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Script;

namespace Waher.Content.Markdown.Contracts.Multimedia
{
    /// <summary>
    /// Web Page content.
    /// </summary>
    public class WebPageContent : Model.Multimedia.WebPageContent, IMultimediaContractsRenderer
    {
        /// <summary>
        /// Web Page content.
        /// </summary>
        public WebPageContent()
        {
        }

		/// <summary>
		/// Generates smart contract XML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderContractXml(ContractsRenderer Renderer, MultimediaItem[] Items, 
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
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
