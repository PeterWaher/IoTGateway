using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Wpf.Multimedia
{
	/// <summary>
	/// Video content.
	/// </summary>
	public class VideoContent : Model.Multimedia.VideoContent, IMultimediaWpfXamlRenderer
	{
		/// <summary>
		/// Video content.
		/// </summary>
		public VideoContent()
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
				Output.WriteStartElement("MediaElement");
				Output.WriteAttributeString("Source", Document.CheckURL(Item.Url, Document.URL));

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
