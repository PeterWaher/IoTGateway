using System.Threading.Tasks;
using System;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.IO;

namespace Waher.Content.Markdown.Model.Multimedia
{
    /// <summary>
    /// Markdown content.
    /// </summary>
    public class MarkdownContent : MultimediaContent
    {
        /// <summary>
        /// Markdown content.
        /// </summary>
        public MarkdownContent()
        {
        }

        /// <summary>
        /// Checks how well the handler supports multimedia content of a given type.
        /// </summary>
        /// <param name="Item">Multimedia item.</param>
        /// <returns>How well the handler supports the content.</returns>
        public override Grade Supports(MultimediaItem Item)
        {
			if (!(Item.Document is null) && !string.IsNullOrEmpty(Item.Document.FileName) &&
				Item.Url.IndexOf(':') < 0 && Item.ContentType == MarkdownCodec.ContentType)
			{
				return Grade.Excellent;
			}
			else
				return Grade.NotAtAll;
        }

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			return false;
		}

		/// <summary>
		/// Gets the parsed Markdown referenced to by a Multi-media item.
		/// </summary>
		/// <param name="Item">Multi-media item.</param>
		/// <param name="ParentURL">URL to parent document.</param>
		/// <returns>Parsed Markdown document.</returns>
		public static async Task<MarkdownDocument> GetMarkdown(MultimediaItem Item, string ParentURL)
		{
			int i = Item.Url.IndexOf('?');
			string Query;
			string FileName;

			if (i < 0)
			{
				Query = null;
				FileName = Item.Url;
			}
			else
			{
				Query = Item.Url.Substring(i + 1);
				FileName = Item.Url.Substring(0, i);
			}

			if (!string.IsNullOrEmpty(ParentURL))
			{
				if (Uri.TryCreate(new Uri(ParentURL), FileName, out Uri NewUri))
					ParentURL = NewUri.ToString();
			}

			MarkdownDocument Document = Item.Document;
			MarkdownSettings Settings = Document.Settings;

			FileName = Settings.GetFileName(Document.FileName, FileName);

			if (!string.IsNullOrEmpty(Query))
			{
				Variables Variables = Settings.Variables;
				string Name;
				string Value;

				if (!(Variables is null))
				{
					foreach (string Part in Query.Split('&'))
					{
						i = Part.IndexOf('=');
						if (i < 0)
							Variables[Part] = string.Empty;
						else
						{
							Name = System.Net.WebUtility.UrlDecode(Part.Substring(0, i));
							Value = System.Net.WebUtility.UrlDecode(Part.Substring(i + 1));

							if (CommonTypes.TryParse(Value, out double d))
								Variables[Name] = d;
							else if (bool.TryParse(Value, out bool b))
								Variables[Name] = b;
							else
								Variables[Name] = Value;
						}
					}
				}
			}

			string MarkdownText = await Files.ReadAllTextAsync(FileName);
			MarkdownDocument Markdown = await MarkdownDocument.CreateAsync(MarkdownText, Settings, FileName, string.Empty, ParentURL);
			Markdown.Master = Document;

			MarkdownDocument Loop = Document;

			while (!(Loop is null))
			{
				if (Loop.FileName == FileName)
					throw new Exception("Circular reference detected.");

				MarkdownDocument.CopyMetaDataTags(Loop, Markdown, false);

				Loop = Loop.Master;
			}

			return Markdown;
		}

		/// <summary>
		/// Includes a markdown document from a file.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public static async Task ProcessInclusion(Renderer Renderer, MultimediaItem[] Items, MarkdownDocument Document)
		{
			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
				{
					MarkdownDocument Markdown = await GetMarkdown(Item, Document.URL);
					await Renderer.RenderDocument(Markdown, true);
				}
			}
			finally
			{
				Variables?.Pop();
			}
		}

	}
}
