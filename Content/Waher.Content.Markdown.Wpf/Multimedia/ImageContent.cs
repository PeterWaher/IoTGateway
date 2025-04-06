using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Wpf.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : Model.Multimedia.ImageContent, IMultimediaWpfXamlRenderer
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderWpfXaml(WpfXamlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				return OutputWpf(Renderer.XmlOutput, new ImageSource()
				{
					Url = Document.CheckURL(Item.Url, null),
					Width = Item.Width,
					Height = Item.Height
				}, Item.Title);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Outputs an image to WPF XAML
		/// </summary>
		/// <param name="Output">WPF XAML output.</param>
		/// <param name="Source">Image source.</param>
		/// <param name="Title">Title of image.</param>
		public static async Task OutputWpf(XmlWriter Output, IImageSource Source, string Title)
		{
			Source = await CheckDataUri(Source);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source.Url);

			bool HasSize = false;

			if (Source.Width.HasValue)
			{
				Output.WriteAttributeString("Width", Source.Width.Value.ToString());
				HasSize = true;
			}

			if (Source.Height.HasValue)
			{
				Output.WriteAttributeString("Height", Source.Height.Value.ToString());
				HasSize = true;
			}

			if (!HasSize)
				Output.WriteAttributeString("Stretch", "None");

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();
		}
	}
}
