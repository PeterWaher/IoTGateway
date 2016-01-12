using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : MultimediaContent
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Url">URL to content.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(string Url)
		{
			string Extension = Path.GetExtension(Url);
			string ContentType;

			if (InternetContent.TryGetContentType(Extension, out ContentType) && ContentType.StartsWith("image/"))
				return Grade.Ok;
			else
				return Grade.Barely;
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
			StringBuilder Alt = new StringBuilder();

			foreach (MarkdownElement E in ChildNodes)
				E.GeneratePlainText(Alt);

			string AltStr = Alt.ToString();

			if (AloneInParagraph)
				Output.Append("<figure>");

			Output.Append("<img src=\"");
			Output.Append(MarkdownDocument.HtmlAttributeEncode(Url));

			Output.Append("\" alt=\"");
			Output.Append(MarkdownDocument.HtmlAttributeEncode(AltStr));

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("\" title=\"");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(Title));
			}

			if (Width.HasValue)
			{
				Output.Append("\" width=\"");
				Output.Append(Width.Value.ToString());
			}

			if (Height.HasValue)
			{
				Output.Append("\" height=\"");
				Output.Append(Height.Value.ToString());
			}

			Output.Append("\"/>");

			if (AloneInParagraph)
			{
				Output.Append("<figurecaption>");
				Output.Append(MarkdownDocument.HtmlValueEncode(AltStr));
				Output.AppendLine("</figurecaption></figure>");
			}
		}
	}
}
