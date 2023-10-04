﻿using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : MultimediaContent
	{
		private static readonly Regex youTubeLink = new Regex(@"^(?'Scheme'http(s)?)://(www[.])?youtube[.]com/watch[?]v=(?'VideoId'[^&].*)", RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Regex youTubeLink2 = new Regex(@"^(?'Scheme'http(s)?)://(www[.])?youtu[.]be/(?'VideoId'[^&].*)", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// YouTube content.
		/// </summary>
		public YouTubeContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (youTubeLink.IsMatch(Item.Url) || youTubeLink2.IsMatch(Item.Url))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}


		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			return true;
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
				bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				Match M = youTubeLink.Match(Item.Url);
				if (!M.Success)
					M = youTubeLink2.Match(Item.Url);

				if (M.Success)
				{
					Output.Append("<iframe src=\"");
					Output.Append(M.Groups["Scheme"].Value);
					Output.Append("://www.youtube.com/embed/");
					Output.Append(XML.HtmlAttributeEncode(M.Groups["VideoId"].Value));

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
						await E.GenerateHTML(Output);

					Output.Append("</iframe>");

					if (AloneInParagraph)
						Output.AppendLine();

					break;
				}
			}
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
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				Output.WriteStartElement("WebBrowser");

				Match M = youTubeLink.Match(Item.Url);
				if (M.Success)
					Output.WriteAttributeString("Source", M.Groups["Scheme"].Value + "://www.youtube.com/embed/" + M.Groups["VideoId"].Value);
				else
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

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				Output.WriteStartElement("WebView");

				Match M = youTubeLink.Match(Item.Url);
				if (M.Success)
					Output.WriteAttributeString("Source", M.Groups["Scheme"].Value + "://www.youtube.com/embed/" + M.Groups["VideoId"].Value);
				else
					Output.WriteAttributeString("Source", Item.Url);

				if (Item.Width.HasValue)
					Output.WriteAttributeString("WidthRequest", Item.Width.Value.ToString());

				if (Item.Height.HasValue)
					Output.WriteAttributeString("HeightRequest", Item.Height.Value.ToString());

				// TODO: Tooltip

				Output.WriteEndElement();

				break;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override Task GenerateLaTeX(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
				Output.AppendLine(InlineText.EscapeLaTeX(Item.Url));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State,
			MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph,
			MarkdownDocument Document)
		{
			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
					await InlineScript.GenerateSmartContractXml(Item.Url, Output, true, Variables, State);
			}
			finally
			{
				Variables?.Pop();
			}
		}
	}
}
