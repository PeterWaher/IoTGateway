using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Rendering.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : Model.Multimedia.TableOfContents, IMultimediaHtmlRenderer, IMultimediaTextRenderer
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
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
		public async Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;
			int LastLevel = 0;
			bool ListItemAdded = true;

			Output.AppendLine("<div class=\"toc\">");
			Output.Append("<div class=\"tocTitle\">");

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(Renderer);

			Output.AppendLine("</div><div class=\"tocBody\">");

			int NrLevel1 = 0;
			bool SkipLevel1;

			foreach (Header Header in Document.Headers)
			{
				if (Header.Level == 1)
					NrLevel1++;
			}

			SkipLevel1 = (NrLevel1 == 1);
			if (SkipLevel1)
				LastLevel++;

			foreach (Header Header in Document.Headers)
			{
				if (SkipLevel1 && Header.Level == 1)
					continue;

				if (Header.Level > LastLevel)
				{
					while (Header.Level > LastLevel)
					{
						if (!ListItemAdded)
						{
							Output.AppendLine();
							Output.Append("<li>");
						}

						Output.Append("<ol>");
						LastLevel++;
						ListItemAdded = false;
					}
				}
				else if (Header.Level < LastLevel)
				{
					while (Header.Level < LastLevel)
					{
						if (ListItemAdded)
							Output.Append("</li>");

						Output.Append("</ol>");
						ListItemAdded = true;
						LastLevel--;
					}
				}

				if (ListItemAdded)
					Output.Append("</li>");

				Output.AppendLine();
				Output.Append("<li><a href=\"#");
				Output.Append(XML.HtmlAttributeEncode(await Header.Id));
				Output.Append("\">");

				foreach (MarkdownElement E in Header.Children)
					await E.Render(Renderer);

				Output.Append("</a>");
				ListItemAdded = true;
			}

			while (LastLevel > (SkipLevel1 ? 1 : 0))
			{
				if (ListItemAdded)
					Output.Append("</li>");

				Output.Append("</ol>");
				ListItemAdded = true;
				LastLevel--;
			}

			if (AloneInParagraph)
				Output.AppendLine();

			Output.AppendLine("</div>");
			Output.AppendLine("</div>");
		}

		/// <summary>
		/// Generates plain text for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderText(TextRenderer Renderer, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			LinkedList<int> Stack = new LinkedList<int>();
			StringBuilder Output = Renderer.Output;
			int LastLevel = 0;
			int ItemNr = 0;
			bool ListItemAdded = true;

			foreach (Header Header in Document.Headers)
			{
				if (Header.Level > LastLevel)
				{
					while (Header.Level > LastLevel)
					{
						if (!ListItemAdded)
						{
							ItemNr++;
							if (LastLevel > 1)
								Output.Append(new string('\t', LastLevel - 1));
							Output.Append(ItemNr.ToString());
							Output.AppendLine(".\t");
						}

						LastLevel++;
						ListItemAdded = false;
						Stack.AddFirst(ItemNr);
						ItemNr = 0;
					}
				}
				else if (Header.Level < LastLevel)
				{
					while (Header.Level < LastLevel)
					{
						ListItemAdded = true;
						LastLevel--;

						ItemNr = Stack.First.Value;
						Stack.RemoveFirst();
					}
				}

				ItemNr++;

				if (LastLevel > 1)
					Output.Append(new string('\t', LastLevel - 1));
				Output.Append(ItemNr.ToString());
				Output.Append(".\t");

				await Renderer.RenderChildren(Header);

				Output.AppendLine();
				ListItemAdded = true;
			}

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
			return Task.CompletedTask;
		}
	}
}
