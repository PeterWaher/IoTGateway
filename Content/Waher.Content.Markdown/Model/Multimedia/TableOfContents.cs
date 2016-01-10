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
	public class TableOfContents : IMultimediaContent
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
		public Grade Supports(string Url)
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
		public void GenerateHTML(StringBuilder Output, string Url, string Title, int? Width, int? Height, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			int LastLevel = 0;
			bool ListItemAdded = true;

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

			Output.AppendLine();
		}
	}
}
