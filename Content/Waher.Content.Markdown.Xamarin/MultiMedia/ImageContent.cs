using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : Model.Multimedia.ImageContent, IMultimediaXamarinFormsXamlRenderer
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				return OutputXamarinForms(Renderer.XmlOutput, new ImageSource()
				{
					Url = Document.CheckURL(Item.Url, null),
					Width = Item.Width,
					Height = Item.Height
				});
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Outputs an image to Xamarin XAML
		/// </summary>
		/// <param name="Output">Xamarin.Forms XAML output.</param>
		/// <param name="Source">Image source.</param>
		public static async Task OutputXamarinForms(XmlWriter Output, IImageSource Source)
		{
			Source = await CheckDataUri(Source);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source.Url);

			if (Source.Width.HasValue)
				Output.WriteAttributeString("WidthRequest", Source.Width.Value.ToString());

			if (Source.Height.HasValue)
				Output.WriteAttributeString("HeightRequest", Source.Height.Value.ToString());

			// TODO: Tooltip

			Output.WriteEndElement();
		}
	}
}
