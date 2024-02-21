using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Content.Markdown.Contracts.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : Model.Multimedia.TableOfContents, IMultimediaContractsRenderer
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}

		/// <summary>
		/// Generates smart contract XML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderContractXml(ContractsRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			XmlWriter Output = Renderer.XmlOutput;
			int LastLevel = 0;
			bool ListItemAdded = true;

			Output.WriteStartElement("paragraph");

			foreach (MarkdownElement E in ChildNodes)
				await E.Render(Renderer);

			Output.WriteEndElement();

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

			int NrHeaders = Document.Headers.Length;
			int HeaderIndex;

			for (HeaderIndex = 0; HeaderIndex < NrHeaders; HeaderIndex++)
			{
				Header Header = Document.Headers[HeaderIndex];

				if (SkipLevel1 && Header.Level == 1)
					continue;

				if (Header.Level > LastLevel)
				{
					while (Header.Level > LastLevel)
					{
						if (!ListItemAdded)
							Output.WriteStartElement("item");

						Output.WriteStartElement("numberedItems");
						LastLevel++;
						ListItemAdded = false;
					}
				}
				else if (Header.Level < LastLevel)
				{
					while (Header.Level < LastLevel)
					{
						if (ListItemAdded)
							Output.WriteEndElement();

						Output.WriteEndElement();
						ListItemAdded = true;
						LastLevel--;
					}
				}

				if (ListItemAdded)
					Output.WriteEndElement();

				Output.WriteStartElement("item");

				if (HeaderIndex + 1 < NrHeaders &&
					Document.Headers[HeaderIndex + 1].Level > Header.Level)
				{
					Output.WriteStartElement("paragraph");
					
					await Renderer.RenderChildren(Header);
					
					Output.WriteEndElement();
				}
				else
					await Renderer.RenderChildren(Header);

				ListItemAdded = true;
			}

			while (LastLevel > (SkipLevel1 ? 1 : 0))
			{
				if (ListItemAdded)
					Output.WriteEndElement();

				Output.WriteEndElement();
				ListItemAdded = true;
				LastLevel--;
			}
		}
	}
}
