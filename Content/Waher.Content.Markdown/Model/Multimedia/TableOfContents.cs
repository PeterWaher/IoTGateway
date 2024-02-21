using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : MultimediaContent
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (string.Compare(Item.Url, "ToC", true) == 0)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
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
