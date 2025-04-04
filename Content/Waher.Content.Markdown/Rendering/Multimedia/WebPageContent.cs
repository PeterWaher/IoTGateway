using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

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
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
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
                
                await Renderer.Render(ChildNodes);

				Output.Append("</iframe>");

                if (AloneInParagraph)
                    Output.AppendLine();

                break;
            }
        }

		/// <summary>
		/// Reports a resource for preloading.
		/// </summary>
		/// <param name="Progress">Progress reporting interface.</param>
		/// <param name="Items">Multi-media items.</param>
		public Task Preload(ICodecProgress Progress, MultimediaItem[] Items)
		{
			if (Items.Length == 1)
			{
				return Progress.EarlyHint(Items[0].Url, "preload",
					new KeyValuePair<string, string>("as", "iframe"));
			}
			else
				return Task.CompletedTask;
		}
	}
}
