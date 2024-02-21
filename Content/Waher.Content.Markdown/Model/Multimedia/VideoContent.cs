using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Video content.
	/// </summary>
	public class VideoContent : MultimediaContent
	{
		/// <summary>
		/// Video content.
		/// </summary>
		public VideoContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (Item.ContentType.StartsWith("video/"))
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
