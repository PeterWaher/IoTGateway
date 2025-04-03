using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
	/// <summary>
	/// Audio content.
	/// </summary>
	public class AudioContent : Model.Multimedia.AudioContent, IMultimediaHtmlRenderer
	{
		/// <summary>
		/// Audio content.
		/// </summary>
		public AudioContent()
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer generating output.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Renderer.Output.Append("<audio");

			if (Document.Settings.AudioAutoplay)
				Renderer.Output.Append(" autoplay=\"autoplay\"");

			if (Document.Settings.AudioControls)
				Renderer.Output.Append(" controls=\"controls\"");

			Renderer.Output.AppendLine(">");

			foreach (MultimediaItem Item in Items)
			{
				Renderer.Output.Append("<source src=\"");
				Renderer.Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Item.Url, Document.URL)));
				Renderer.Output.Append("\" type=\"");
				Renderer.Output.Append(XML.HtmlAttributeEncode(Item.ContentType));
				Renderer.Output.AppendLine("\"/>");
			}

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(Renderer);

			Renderer.Output.AppendLine("</audio>");
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
					new KeyValuePair<string, string>("as", "audio"));
			}
			else
				return Task.CompletedTask;
		}
	}
}
