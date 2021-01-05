using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : MultimediaContent
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (string.Compare(Item.Url, "ToC", true) == 0)
				return Grade.Excellent;
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
			int LastLevel = 0;
			bool ListItemAdded = true;

			Output.AppendLine("<div class=\"toc\">");
			Output.Append("<div class=\"tocTitle\">");

			foreach (MarkdownElement E in ChildNodes)
				E.GenerateHTML(Output);

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
				Output.Append(XML.HtmlAttributeEncode(Header.Id));
				Output.Append("\">");

				foreach (MarkdownElement E in Header.Children)
					E.GenerateHTML(Output);

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
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GeneratePlainText(StringBuilder Output, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			LinkedList<int> Stack = new LinkedList<int>();
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

				foreach (MarkdownElement E in Header.Children)
					E.GeneratePlainText(Output);

				Output.AppendLine();
				ListItemAdded = true;
			}

			if (AloneInParagraph)
				Output.AppendLine();
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
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items, 
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			// TODO: Table of Contents in XAML
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			// TODO: Table of Contents in Xamarin.Forms
		}

	}
}
