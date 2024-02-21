using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
    /// <summary>
    /// Web Page content.
    /// </summary>
    public class WebPageContent : Model.Multimedia.WebPageContent, IMultimediaHtmlRenderer
    {
        /// <summary>
        /// Web Page content.
        /// </summary>
        public WebPageContent()
        {
        }

		/// <summary>
		/// Generates HTML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
            StringBuilder Output = Renderer.Output;

            foreach (MultimediaItem Item in Items)
            {
                Output.Append("<iframe src=\"");
                Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Item.Url, null)));

                if (Item.Width.HasValue)
                {
                    Output.Append("\" width=\"");
                    Output.Append(Item.Width.Value.ToString());
                }

                if (Item.Height.HasValue)
                {
                    Output.Append("\" height=\"");
                    Output.Append(Item.Height.Value.ToString());
                }

                Output.Append("\">");

                foreach (MarkdownElement E in ChildNodes)
                    await E.Render(Renderer);

                Output.Append("</iframe>");

                if (AloneInParagraph)
                    Output.AppendLine();

                break;
            }
        }
	}
}
