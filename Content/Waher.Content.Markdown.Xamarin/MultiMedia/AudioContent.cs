using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
	/// <summary>
	/// Audio content.
	/// </summary>
	public class AudioContent : Model.Multimedia.AudioContent, IMultimediaXamarinFormsXamlRenderer
	{
		/// <summary>
		/// Audio content.
		/// </summary>
		public AudioContent()
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				Renderer.XmlOutput.WriteStartElement("MediaElement");
				Renderer.XmlOutput.WriteAttributeString("Source", Document.CheckURL(Item.Url, Document.URL));
				Renderer.XmlOutput.WriteAttributeString("AutoPlay", "True");

				// TODO: Tooltip

				Renderer.XmlOutput.WriteEndElement();

				break;
			}

			return Task.CompletedTask;
		}
	}
}
