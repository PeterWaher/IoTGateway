using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script;

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
			if (Item.Document != null && !string.IsNullOrEmpty(Item.Document.FileName) &&
				Item.Url.IndexOf(':') < 0 && Item.ContentType == "text/markdown")
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
		public override void GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            if (Variables != null)
                Variables.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = this.GetMarkdown(Item, Document.URL);
                    Markdown.GenerateHTML(Output, true);

                    if (AloneInParagraph)
                        Output.AppendLine();
                }
            }
            finally
            {
                if (Variables != null)
                    Variables.Pop();
            }
        }

        private MarkdownDocument GetMarkdown(MultimediaItem Item, string ParentURL)
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

			FileName = Path.Combine(Path.GetDirectoryName(Item.Document.FileName), FileName);

			if (!string.IsNullOrEmpty(Query))
			{
				Variables Variables = Item.Document.Settings.Variables;
				string Value;
				
				if (Variables != null)
				{
					foreach (string Part in Query.Split('&'))
					{
						i = Part.IndexOf('=');
						if (i < 0)
							Variables[Part] = string.Empty;
						else
						{
							Value = Part.Substring(i + 1);

							if (double.TryParse(Value.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out double d))
								Variables[Part.Substring(0, i)] = d;
							else if (bool.TryParse(Value, out bool b))
								Variables[Part.Substring(0, i)] = b;
							else
								Variables[Part.Substring(0, i)] = Value;
						}
					}
				}
			}

			string MarkdownText = File.ReadAllText(FileName);
			MarkdownDocument Markdown = new MarkdownDocument(MarkdownText, Item.Document.Settings, FileName, string.Empty, ParentURL)
			{
				Master = Item.Document
			};

            MarkdownDocument Loop = Item.Document;

			while (Loop != null)
            {
                if (Loop.FileName == FileName)
                    throw new Exception("Circular reference detected.");

				MarkdownDocument.CopyMetaDataTags(Loop, Markdown);

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
		public override void GeneratePlainText(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
            bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            if (Variables != null)
                Variables.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = this.GetMarkdown(Item, Document.URL);
                    Markdown.GeneratePlainText(Output);

                    if (AloneInParagraph)
                        Output.AppendLine();
                }
            }
            finally
            {
                if (Variables != null)
                    Variables.Pop();
            }
        }

        /// <summary>
        /// Generates XAML for the markdown element.
        /// </summary>
        /// <param name="Output">XAML will be output here.</param>
        /// <param name="Settings">XAML settings.</param>
        /// <param name="TextAlignment">Alignment of text in element.</param>
        /// <param name="Items">Multimedia items.</param>
        /// <param name="ChildNodes">Child nodes.</param>
        /// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
        /// <param name="Document">Markdown document containing element.</param>
        public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment, MultimediaItem[] Items,
            IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
        {
            Variables Variables = Document.Settings.Variables;
            if (Variables != null)
                Variables.Push();

            try
            {
                foreach (MultimediaItem Item in Items)
                {
                    MarkdownDocument Markdown = this.GetMarkdown(Item, Document.URL);
                    Markdown.GenerateXAML(Output, Settings, true);
                }
            }
            finally
            {
                if (Variables != null)
                    Variables.Pop();
            }
        }
    }
}
