using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Wpf.Multimedia
{
	/// <summary>
	/// Audio content.
	/// </summary>
	public class AudioContent : Model.Multimedia.AudioContent, IMultimediaWpfXamlRenderer
	{
		/// <summary>
		/// Audio content.
		/// </summary>
		public AudioContent()
		{
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderWpfXaml(WpfXamlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				Renderer.XmlOutput.WriteStartElement("MediaElement");
				Renderer.XmlOutput.WriteAttributeString("Source", Document.CheckURL(Item.Url, Document.URL));
				Renderer.XmlOutput.WriteAttributeString("LoadedBehavior", "Play");

				if (!string.IsNullOrEmpty(Item.Title))
					Renderer.XmlOutput.WriteAttributeString("ToolTip", Item.Title);

				Renderer.XmlOutput.WriteEndElement();

				break;
			}
		
			return Task.CompletedTask;
		}
	}
}
