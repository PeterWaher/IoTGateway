using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Script;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Audio content.
	/// </summary>
	public class AudioContent : MultimediaContent
	{
		/// <summary>
		/// Audio content.
		/// </summary>
		public AudioContent()
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

			if (InternetContent.TryGetContentType(Extension, out ContentType) && ContentType.StartsWith("audio/"))
				return Grade.Ok;
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
			Output.AppendLine("<audio autoplay=\"autoplay\">");

			foreach (MultimediaItem Item in Items)
			{
				Output.Append("<source src=\"");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(Item.Url));
				Output.Append("\" type=\"");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(InternetContent.GetContentType(Path.GetExtension(Item.Url))));
				Output.AppendLine("\"/>");
			}

			foreach (MarkdownElement E in ChildNodes)
				E.GenerateHTML(Output);

			Output.AppendLine("</audio>");
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
				Output.WriteStartElement("MediaElement");
				Output.WriteAttributeString("Source", Item.Url);
				Output.WriteAttributeString("LoadedBehavior", "Play");

				if (!string.IsNullOrEmpty(Item.Title))
					Output.WriteAttributeString("ToolTip", Item.Title);

				Output.WriteEndElement();

				break;
			}
		}
	}
}
