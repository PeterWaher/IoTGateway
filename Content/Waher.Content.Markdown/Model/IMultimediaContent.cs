using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for all markdown handlers of multimedia content.
	/// </summary>
	public interface IMultimediaContent
	{
		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		Grade Supports(MultimediaItem Item);

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		bool EmbedInlineLink(string Url);
	}
}
