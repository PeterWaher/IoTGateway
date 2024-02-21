using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for multimedia content.
	/// </summary>
	public abstract class MultimediaContent : IMultimediaContent
	{
		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public abstract Grade Supports(MultimediaItem Item);

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public abstract bool EmbedInlineLink(string Url);
	}
}
