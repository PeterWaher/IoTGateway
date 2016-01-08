using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Script;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : IMultimediaContent
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
		public Grade Supports(string Url)
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
		public void GenerateHTML(StringBuilder Output, string Url, string Title, int? Width, int? Height, IEnumerable<MarkdownElement> ChildNodes)
		{
			Match M = youTubeLink.Match(Url);
			if (M.Success)
			{
				Output.Append("<iframe src=\"http://www.youtube.com/embed/");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(M.Groups["VideoId"].Value));
				Output.Append("?autoplay=1");

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
			}
		}
	}
}
