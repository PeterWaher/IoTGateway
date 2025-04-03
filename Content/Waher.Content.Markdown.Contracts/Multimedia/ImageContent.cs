using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;
using Waher.Runtime.Temporary;
using Waher.Script;

namespace Waher.Content.Markdown.Contracts.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : Model.Multimedia.ImageContent, IMultimediaContractsRenderer
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
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
		public async Task RenderContractXml(ContractsRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			try
			{
				XmlWriter Output = Renderer.XmlOutput;
				string ContentType;
				string Title;
				byte[] Bin;
				int Width;
				int Height;

				foreach (MultimediaItem Item in Items)
				{
					try
					{
						Uri Uri = new Uri(Item.Url);
						using (ContentStreamResponse P = await InternetContent.GetTempStreamAsync(
							new Uri(Item.Url), 10000, new KeyValuePair<string, string>("Accept", "image/*")))
						{
							TemporaryStream f = P.Encoded;
							MemoryStream ms = new MemoryStream();
							f.Position = 0;

							await f.CopyToAsync(ms);

							Bin = ms.ToArray();
							ContentType = P.ContentType;

							ContentResponse DecodedItem = await InternetContent.DecodeAsync(ContentType, Bin, Uri);
							if (DecodedItem.HasError || !(DecodedItem.Decoded is SKImage Image))
								continue;

							Width = Item.Width ?? Image.Width;
							Height = Item.Height ?? Image.Height;
							Title = Item.Title;

							if (AloneInParagraph)
								Output.WriteStartElement("imageStandalone");
							else
								Output.WriteStartElement("imageInline");

							Output.WriteAttributeString("contentType", ContentType);
							Output.WriteAttributeString("width", Image.Width.ToString());
							Output.WriteAttributeString("height", Image.Height.ToString());

							Output.WriteStartElement("binary");
							Output.WriteValue(Convert.ToBase64String(Bin));
							Output.WriteEndElement();

							Output.WriteStartElement("caption");
							if (string.IsNullOrEmpty(Title))
							{
								bool Found = false;

								if (!(ChildNodes is null))
								{
									foreach (MarkdownElement E in ChildNodes)
									{
										await E.Render(Renderer);
										Found = true;
									}
								}

								if (!Found)
									Output.WriteElementString("text", "Image");
							}
							else
								Output.WriteElementString("text", Title);

							Output.WriteEndElement();
							Output.WriteEndElement();

							return;
						}
					}
					catch (Exception)
					{
						continue;
					}
				}
			}
			catch (Exception)
			{
			}

			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
					await Renderer.RenderObject(Item.Url, AloneInParagraph, Variables);
			}
			finally
			{
				Variables?.Pop();
			}

		}
	}
}
