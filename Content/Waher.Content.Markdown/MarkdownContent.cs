using Waher.Content.Json;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown
{
    /// <summary>
    /// Class that can be used to encapsulate Markdown to be returned from a Web Service, bypassing any encoding protections,
    /// and avoiding doubly parsing the Markdown.
    /// </summary>
    public class MarkdownContent : IJsonEncodingHint
	{
		private readonly string markdown;

		/// <summary>
		/// Class that can be used to encapsulate Markdown to be returned from a Web Service, bypassing any encoding protections,
		/// and avoiding doubly parsing the Markdown.
		/// </summary>
		/// <param name="Markdown">Markdown content to return.</param>
		public MarkdownContent(string Markdown)
		{
			this.markdown = Markdown;
		}

		/// <summary>
		/// Markdown content.
		/// </summary>
		public string Markdown => this.markdown;

		/// <summary>
		/// To what extent the object supports JSON encoding.
		/// </summary>
		public Grade CanEncodeJson => Grade.NotAtAll;
	}
}
