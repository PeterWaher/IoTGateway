using System.Text.RegularExpressions;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// YouTube content.
	/// </summary>
	public class YouTubeContent : MultimediaContent
	{
		/// <summary>
		/// https://youtube.com/watch?v=...
		/// </summary>
		protected static readonly Regex youTubeLink = new Regex(@"^(?'Scheme'http(s)?)://(www[.])?youtube[.]com/watch[?]v=(?'VideoId'[^&].*)", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// https://youtu.be/...
		/// </summary>
		protected static readonly Regex youTubeLink2 = new Regex(@"^(?'Scheme'http(s)?)://(www[.])?youtu[.]be/(?'VideoId'[^&].*)", RegexOptions.Singleline | RegexOptions.Compiled);

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
	}
}
