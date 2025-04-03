using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : Model.Multimedia.ImageContent, IMultimediaHtmlRenderer
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Renderer">Renderer</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			using (TextRenderer Alt = new TextRenderer())
			{
				bool SizeSet;

				foreach (MarkdownElement E in ChildNodes)
					await E.Render(Alt);

				StringBuilder Output = Renderer.Output;
				string AltStr = Alt.ToString();
				bool SameWidth = true;
				bool SameHeight = true;

				if (string.IsNullOrEmpty(AltStr))
					AloneInParagraph = false;

				if (AloneInParagraph)
					Output.Append("<figure>");

				if (Items.Length > 1)
				{
					Output.AppendLine("<picture>");

					foreach (MultimediaItem Item in Items)
					{
						if (Item.Width != Items[0].Width)
							SameWidth = false;

						if (Item.Height != Items[0].Height)
							SameHeight = false;

						Output.Append("<source srcset=\"");
						Output.Append(XML.HtmlAttributeEncode(Item.Url));
						Output.Append("\" type=\"");
						Output.Append(XML.HtmlAttributeEncode(Item.ContentType));

						if (Item.Width.HasValue)
						{
							Output.Append("\" media=\"(min-width:");
							Output.Append(Item.Width.Value.ToString());
							Output.Append("px)");
						}

						Output.AppendLine("\"/>");
					}
				}

				Output.Append("<img src=\"");
				Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Items[0].Url, null)));

				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(AltStr));

				if (!string.IsNullOrEmpty(Items[0].Title))
				{
					Output.Append("\" title=\"");
					Output.Append(XML.HtmlAttributeEncode(Items[0].Title));
				}

				SizeSet = false;

				if (SameWidth && Items[0].Width.HasValue)
				{
					Output.Append("\" width=\"");
					Output.Append(Items[0].Width.Value.ToString());
					SizeSet = true;
				}

				if (SameHeight && Items[0].Height.HasValue)
				{
					Output.Append("\" height=\"");
					Output.Append(Items[0].Height.Value.ToString());
					SizeSet = true;
				}

				if (AloneInParagraph && !SizeSet && Items.Length == 1)
					Output.Append("\" class=\"aloneUnsized");

				if (Items.Length > 1)
				{
					Output.Append("\" srcset=\"");

					bool First = true;

					foreach (MultimediaItem Item in Items)
					{
						if (First)
							First = false;
						else
							Output.Append(", ");

						Output.Append(XML.HtmlAttributeEncode(Item.Url));

						if (Item.Width.HasValue)
						{
							Output.Append(' ');
							Output.Append(Item.Width.Value.ToString());
							Output.Append('w');
						}
					}

					Output.Append("\" sizes=\"100vw");
				}

				Output.Append("\"/>");

				if (Items.Length > 1)
					Output.AppendLine("</picture>");

				if (AloneInParagraph)
				{
					Output.Append("<figcaption>");
					Output.Append(XML.HtmlValueEncode(AltStr));
					Output.AppendLine("</figcaption></figure>");
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
			if (Items.Length == 1)
			{
				return Progress.EarlyHint(Items[0].Url, "preload",
					new KeyValuePair<string, string>("as", "image"));
			}
			else
				return Task.CompletedTask;
		}
	}
}
