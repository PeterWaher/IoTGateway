using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
	/// <summary>
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : Model.Multimedia.YouTubeContent, IMultimediaHtmlRenderer
	{
		/// <summary>
		/// YouTube content.
		/// </summary>
		public YouTubeContent()
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

			foreach (MultimediaItem Item in Items)
			{
				Match M = youTubeLink.Match(Item.Url);
				if (!M.Success)
					M = youTubeLink2.Match(Item.Url);

				if (M.Success)
				{
					Output.Append("<iframe src=\"");
					Output.Append(M.Groups["Scheme"].Value);
					Output.Append("://www.youtube.com/embed/");
					Output.Append(XML.HtmlAttributeEncode(M.Groups["VideoId"].Value));

					if (Item.Width.HasValue)
					{
						Output.Append("\" width=\"");
						Output.Append(Item.Width.Value.ToString());
					}

					if (Item.Height.HasValue)
					{
						Output.Append("\" height=\"");
						Output.Append(Item.Height.Value.ToString());
					}

					Output.Append("\">");

					foreach (MarkdownElement E in ChildNodes)
						await E.Render(Renderer);

					Output.Append("</iframe>");

					if (AloneInParagraph)
						Output.AppendLine();

					break;
				}
			}
		}

		/// <summary>
		/// Reports a resource for preloading.
		/// </summary>
		/// <param name="Progress">Progress reporting interface.</param>
		/// <param name="Items">Multi-media items.</param>
		public Task Preload(ICodecProgress Progress, MultimediaItem[] Items)
		{
			return Task.CompletedTask;
		}
	}
}
