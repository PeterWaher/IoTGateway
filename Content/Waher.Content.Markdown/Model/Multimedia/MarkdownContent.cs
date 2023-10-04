using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script;
using System.Threading.Tasks;

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
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            Variables?.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
                    await Markdown.GenerateHTML(Output, true);

                    if (AloneInParagraph)
                        Output.AppendLine();
                }
            }
            finally
            {
                Variables?.Pop();
            }
        }

        private async Task<MarkdownDocument> GetMarkdown(MultimediaItem Item, string ParentURL)
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

			FileName = Item.Document.Settings.GetFileName(Item.Document.FileName, FileName);

			if (!string.IsNullOrEmpty(Query))
			{
				Variables Variables = Item.Document.Settings.Variables;
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
							Value = Part.Substring(i + 1);

							if (CommonTypes.TryParse(Value, out double d))
								Variables[Part.Substring(0, i)] = d;
							else if (bool.TryParse(Value, out bool b))
								Variables[Part.Substring(0, i)] = b;
							else
								Variables[Part.Substring(0, i)] = Value;
						}
					}
				}
			}

			string MarkdownText = await Resources.ReadAllTextAsync(FileName);
            MarkdownDocument Markdown = await MarkdownDocument.CreateAsync(MarkdownText, Item.Document.Settings, FileName, string.Empty, ParentURL);
            Markdown.Master = Item.Document;

            MarkdownDocument Loop = Item.Document;

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
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GeneratePlainText(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            Variables?.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
                    await Markdown.GeneratePlainText(Output);

                    if (AloneInParagraph)
                        Output.AppendLine();
                }
            }
            finally
            {
                Variables?.Pop();
            }
        }

        /// <summary>
        /// Generates WPF XAML for the markdown element.
        /// </summary>
        /// <param name="Output">XAML will be output here.</param>
        /// <param name="TextAlignment">Alignment of text in element.</param>
        /// <param name="Items">Multimedia items.</param>
        /// <param name="ChildNodes">Child nodes.</param>
        /// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
        /// <param name="Document">Markdown document containing element.</param>
        public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
            IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            Variables?.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
                    await Markdown.GenerateXAML(Output, true);
                }
            }
            finally
            {
                Variables?.Pop();
            }
        }

        /// <summary>
        /// Generates Xamarin.Forms XAML for the markdown element.
        /// </summary>
        /// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
        /// <param name="Items">Multimedia items.</param>
        /// <param name="ChildNodes">Child nodes.</param>
        /// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
        /// <param name="Document">Markdown document containing element.</param>
        public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, MultimediaItem[] Items,
            IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            Variables?.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
                    await Markdown.GenerateXamarinForms(Output, true);
                }
            }
            finally
            {
                Variables?.Pop();
            }
        }

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateLaTeX(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
				{
					MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
					await Markdown.GenerateLaTeX(Output);

					if (AloneInParagraph)
						Output.AppendLine();
				}
			}
			finally
			{
				Variables?.Pop();
			}
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State,
			MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
				{
					MarkdownDocument Markdown = await this.GetMarkdown(Item, Document.URL);
					await Markdown.GenerateSmartContractXml(Output);
				}
			}
			finally
			{
				Variables?.Pop();
			}
		}

	}
}
