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
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : MultimediaContent
	{
		private Regex youTubeLink = new Regex(@"^http(s)?://www[.]youtube[.]com/watch[?]v=(?'VideoId'[^&].*)", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// YouTube content.
		/// </summary>
		public YouTubeContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Url">URL to content.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(string Url)
		{
			if (youTubeLink.IsMatch(Url))
				return Grade.Ok;
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
			Match M = youTubeLink.Match(Url);
			if (M.Success)
			{
				Output.Append("<iframe src=\"http://www.youtube.com/embed/");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(M.Groups["VideoId"].Value));

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

				Output.Append("\">");

				foreach (MarkdownElement E in ChildNodes)
					E.GenerateHTML(Output);

				Output.Append("</iframe>");

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
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="Width">Optional width.</param>
		/// <param name="Height">Optional height.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment, string Url, string Title, int? Width, int? Height, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			Output.WriteStartElement("WebBrowser");

			Match M = youTubeLink.Match(Url);
			if (M.Success)
				Output.WriteAttributeString("Source", "http://www.youtube.com/embed/" + M.Groups["VideoId"].Value);
			else
				Output.WriteAttributeString("Source", Url);

			if (Width.HasValue)
				Output.WriteAttributeString("Width", Width.Value.ToString());

			if (Height.HasValue)
				Output.WriteAttributeString("Height", Height.Value.ToString());

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();
		}
	}
}
