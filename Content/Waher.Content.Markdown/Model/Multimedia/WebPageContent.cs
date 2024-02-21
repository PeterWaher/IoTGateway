using Waher.Runtime.Inventory;

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
            if (Item.Url.EndsWith("/") || Item.ContentType.StartsWith("text/"))
                return Grade.Ok;
            else
                return Grade.Barely;
        }

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			return false;
		}
	}
}
