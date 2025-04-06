using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
	/// <summary>
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : Model.Multimedia.YouTubeContent, IMultimediaXamarinFormsXamlRenderer
	{
		/// <summary>
		/// YouTube content.
		/// </summary>
		public YouTubeContent()
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
			ChunkedList<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			XmlWriter Output = Renderer.XmlOutput;

			foreach (MultimediaItem Item in Items)
			{
				Output.WriteStartElement("WebView");

				Match M = youTubeLink.Match(Item.Url);
				if (M.Success)
					Output.WriteAttributeString("Source", M.Groups["Scheme"].Value + "://www.youtube.com/embed/" + M.Groups["VideoId"].Value);
				else
					Output.WriteAttributeString("Source", Item.Url);

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
