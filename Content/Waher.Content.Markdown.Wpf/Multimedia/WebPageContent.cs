using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Wpf.Multimedia
{
    /// <summary>
    /// Web Page content.
    /// </summary>
    public class WebPageContent : Model.Multimedia.WebPageContent, IMultimediaWpfXamlRenderer
    {
        /// <summary>
        /// Web Page content.
        /// </summary>
        public WebPageContent()
        {
        }

		/// <summary>
		/// Generates WPF XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderWpfXaml(WpfXamlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, 
            bool AloneInParagraph, MarkdownDocument Document)
        {
            XmlWriter Output = Renderer.XmlOutput;

            foreach (MultimediaItem Item in Items)
            {
                Output.WriteStartElement("WebBrowser");
                Output.WriteAttributeString("Source", Document.CheckURL(Item.Url, null));

                if (Item.Width.HasValue)
                    Output.WriteAttributeString("Width", Item.Width.Value.ToString());

                if (Item.Height.HasValue)
                    Output.WriteAttributeString("Height", Item.Height.Value.ToString());

                if (!string.IsNullOrEmpty(Item.Title))
                    Output.WriteAttributeString("ToolTip", Item.Title);

                Output.WriteEndElement();

                break;
            }
        
			return Task.CompletedTask;
        }
	}
}
