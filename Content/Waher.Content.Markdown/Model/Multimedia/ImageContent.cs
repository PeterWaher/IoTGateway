using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
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
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Alt = new StringBuilder();

			foreach (MarkdownElement E in ChildNodes)
				E.GeneratePlainText(Alt);

			string AltStr = Alt.ToString();

			if (AloneInParagraph)
				Output.Append("<figure>");

			if (Items.Length > 1)
			{
				Output.AppendLine("<picture>");

				foreach (MultimediaItem Item in Items)
				{
					Output.Append("<source srcset=\"");
					Output.Append(MarkdownDocument.HtmlAttributeEncode(Item.Url));
					Output.Append("\" type=\"");
					Output.Append(MarkdownDocument.HtmlAttributeEncode(InternetContent.GetContentType(Path.GetExtension(Item.Url))));

					if (Item.Width.HasValue)
					{
						Output.Append("\" media=\"min-width:");
						Output.Append(Item.Width.Value.ToString());
						Output.Append("px");
					}

					Output.AppendLine("\"/>");
				}
			}

			Output.Append("<img src=\"");
			Output.Append(MarkdownDocument.HtmlAttributeEncode(Items[0].Url));

			Output.Append("\" alt=\"");
			Output.Append(MarkdownDocument.HtmlAttributeEncode(AltStr));

			if (!string.IsNullOrEmpty(Items[0].Title))
			{
				Output.Append("\" title=\"");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(Items[0].Title));
			}

			if (Items[0].Width.HasValue)
			{
				Output.Append("\" width=\"");
				Output.Append(Items[0].Width.Value.ToString());
			}

			if (Items[0].Height.HasValue)
			{
				Output.Append("\" height=\"");
				Output.Append(Items[0].Height.Value.ToString());
			}

			Output.Append("\"/>");

			if (Items.Length > 1)
				Output.AppendLine("</picture>");

			if (AloneInParagraph)
			{
				Output.Append("<figcaption>");
				Output.Append(MarkdownDocument.HtmlValueEncode(AltStr));
				Output.AppendLine("</figcaption></figure>");
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
				Output.WriteStartElement("Image");
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
