using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
    /// <summary>
    /// Web Page content.
    /// </summary>
    public class WebPageContent : Model.Multimedia.WebPageContent, IMultimediaXamarinFormsXamlRenderer
    {
        /// <summary>
        /// Web Page content.
        /// </summary>
        public WebPageContent()
        {
        }

		/// <summary>
		/// Generates Xamarin.Forms XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, 
            IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
        {
            XmlWriter Output = Renderer.XmlOutput;

            foreach (MultimediaItem Item in Items)
            {
                Output.WriteStartElement("WebView");
                Output.WriteAttributeString("Source", Document.CheckURL(Item.Url, null));

                if (Item.Width.HasValue)
                    Output.WriteAttributeString("WidthRequest", Item.Width.Value.ToString());

                if (Item.Height.HasValue)
                    Output.WriteAttributeString("HeightRequest", Item.Height.Value.ToString());

                // TODO: Tooltip

                Output.WriteEndElement();

                break;
            }
        
			return Task.CompletedTask;
        }
	}
}
