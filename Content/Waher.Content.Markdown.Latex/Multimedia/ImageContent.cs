﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.IO;

namespace Waher.Content.Markdown.Latex.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : Model.Multimedia.ImageContent, IMultimediaLatexRenderer
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates LaTeX for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public async Task RenderLatex(LatexRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;

			foreach (MultimediaItem Item in Items)
			{
				string Url = Document.CheckURL(Item.Url, Document.URL);

				if (Uri.TryCreate(Url, UriKind.RelativeOrAbsolute, out Uri ParsedUri))
				{
					Stream f;

					if (ParsedUri.IsAbsoluteUri)
					{
						ContentStreamResponse P = await InternetContent.GetTempStreamAsync(new Uri(Item.Url), 60000);
						if (P.HasError)
							continue;

						f = P.Encoded;
					}
					else
					{
						string FileName = Document.Settings.GetFileName(Document.FileName, Url);
						if (!File.Exists(FileName))
							continue;

						f = File.OpenRead(FileName);
					}

					try
					{
						byte[] Bin = await f.ReadAllAsync();
						string FileName = await GetTemporaryFile(Bin);

						if (AloneInParagraph)
						{
							Output.AppendLine("\\begin{figure}[h]");
							Output.AppendLine("\\centering");
						}

						Output.Append("\\fbox{\\includegraphics");

						if (Item.Width.HasValue || Item.Height.HasValue)
						{
							Output.Append('[');

							if (Item.Width.HasValue)
							{
								Output.Append("width=");
								Output.Append(((Item.Width.Value * 3) / 4).ToString());
								Output.Append("pt");
							}

							if (Item.Height.HasValue)
							{
								if (Item.Width.HasValue)
									Output.Append(", ");

								Output.Append("height=");
								Output.Append(((Item.Height.Value * 3) / 4).ToString());
								Output.Append("pt");
							}

							Output.Append(']');
						}

						Output.Append('{');
						Output.Append(FileName.Replace('\\', '/'));
						Output.Append("}}");

						if (AloneInParagraph)
						{
							Output.AppendLine("\\end{figure}");
							Output.AppendLine();
						}
					}
					finally
					{
						f.Dispose();
					}
				}
			}
		}
	}
}
