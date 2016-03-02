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
	/// Web Page content.
	/// </summary>
	public class WebPageContent : MultimediaContent
	{
		/// <summary>
		/// Web Page content.
		/// </summary>
		public WebPageContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (Item.Url.EndsWith("/") || Item.ContentType.StartsWith("image/"))
				return Grade.Ok;
			else
				return Grade.Barely;
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
				Output.Append("<iframe src=\"");
				Output.Append(XML.HtmlAttributeEncode(Item.Url));

				if (Item.Width.HasValue)
				{
					Output.Append("\" width=\"");
					Output.Append(Item.Width.Value.ToString());
				}

				if (Item.Height.HasValue)
				{
					Output.Append("\" height=\"");
					Output.Append(Item.Height.Value.ToString());
				}
				
				Output.Append("\">");

				foreach (MarkdownElement E in ChildNodes)
					E.GenerateHTML(Output);

				Output.Append("</iframe>");

				if (AloneInParagraph)
					Output.AppendLine();

				break;
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
				Output.WriteStartElement("WebBrowser");
				Output.WriteAttributeString("Source", Item.Url);

				if (Item.Width.HasValue)
					Output.WriteAttributeString("Width", Item.Width.Value.ToString());

				if (Item.Height.HasValue)
					Output.WriteAttributeString("Height", Item.Height.Value.ToString());

				if (!string.IsNullOrEmpty(Item.Title))
					Output.WriteAttributeString("ToolTip", Item.Title);

				Output.WriteEndElement();

				break;
			}
		}
	}
}
