using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
	/// <summary>
	/// Video content.
	/// </summary>
	public class VideoContent : Model.Multimedia.VideoContent, IMultimediaHtmlRenderer
	{
		/// <summary>
		/// Video content.
		/// </summary>
		public VideoContent()
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
			bool First = true;

			Output.Append("<video");

			if (Document.Settings.VideoAutoplay)
				Output.Append(" autoplay=\"autoplay\"");

			if (Document.Settings.VideoControls)
				Output.Append(" controls=\"controls\"");

			foreach (MultimediaItem Item in Items)
			{
				if (First)
				{
					First = false;

					if (Item.Width.HasValue)
					{
						Output.Append(" width=\"");
						Output.Append(Item.Width.Value.ToString());
						Output.Append("\"");
					}

					if (Item.Height.HasValue)
					{
						Output.Append(" height=\"");
						Output.Append(Item.Height.Value.ToString());
						Output.Append("\"");
					}

					Output.AppendLine(">");
				}

				Output.Append("<source src=\"");
				Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Item.Url, Document.URL)));
				Output.Append("\" type=\"");
				Output.Append(XML.HtmlAttributeEncode(Item.ContentType));

				Output.AppendLine("\"/>");
			}

			if (First)
				Output.AppendLine(">");

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(Renderer);

			Output.Append("</video>");

			if (AloneInParagraph)
				Output.AppendLine();
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
					new KeyValuePair<string, string>("as", "video"));
			}
			else
				return Task.CompletedTask;
		}
	}
}
