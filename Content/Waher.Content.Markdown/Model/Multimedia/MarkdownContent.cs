using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
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
			if (!string.IsNullOrEmpty(Item.Document.FileName) && Item.Url.IndexOf(':') < 0 && Item.ContentType == "text/markdown")
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
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
			foreach (MultimediaItem Item in Items)
			{
				MarkdownDocument Markdown = this.GetMarkdown(Item);
				Markdown.GenerateHTML(Output, true);

				if (AloneInParagraph)
					Output.AppendLine();
			}
		}

		private MarkdownDocument GetMarkdown(MultimediaItem Item)
		{
			string FileName = Path.Combine(Path.GetDirectoryName(Item.Document.FileName), Item.Url);
			string MarkdownText = File.ReadAllText(FileName);
			
			MarkdownSettings Settings = new MarkdownSettings(Item.Document.EmojiSource, false);
			MarkdownDocument Markdown = new MarkdownDocument(MarkdownText, Settings);
			Markdown.FileName = FileName;
			Markdown.Master = Item.Document;

			MarkdownDocument Loop = Item.Document;
			while (Loop != null)
			{
				if (Loop.FileName == FileName)
					throw new Exception("Circular reference detected.");

				Loop = Loop.Master;
			}

			return Markdown;
		}

		public override void GeneratePlainText(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				MarkdownDocument Markdown = this.GetMarkdown(Item);
				Markdown.GeneratePlainText(Output);

				if (AloneInParagraph)
					Output.AppendLine();
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
			foreach (MultimediaItem Item in Items)
			{
				MarkdownDocument Markdown = this.GetMarkdown(Item);
				Markdown.GenerateXAML(Output, Settings, true);
			}
		}
	}
}
