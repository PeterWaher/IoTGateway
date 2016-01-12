using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Script;
using Waher.Content.Markdown.Model.BlockElements;

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
		/// <param name="Url">URL to content.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(string Url)
		{
			if (string.Compare(Url, "ToC", true) == 0)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="Width">Optional width.</param>
		/// <param name="Height">Optional height.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateHTML(StringBuilder Output, string Url, string Title, int? Width, int? Height, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			int LastLevel = 0;
			bool ListItemAdded = true;

			Output.AppendLine("<div class=\"toc\">");

			foreach (Header Header in Document.Headers)
			{
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
				Output.Append(MarkdownDocument.HtmlAttributeEncode(Header.Id));
				Output.Append("\">");

				foreach (MarkdownElement E in Header.Children)
					E.GenerateHTML(Output);

				Output.Append("</a>");
				ListItemAdded = true;
			}

			while (LastLevel > 0)
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
		}

		public override void GeneratePlainText(StringBuilder Output, string Url, string Title, int? Width, int? Height,
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
	}
}
