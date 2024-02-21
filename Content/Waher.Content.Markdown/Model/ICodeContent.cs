using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for all markdown handlers of code content.
	/// </summary>
	public interface ICodeContent
	{
		/// <summary>
		/// Checks how well the handler supports code content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		Grade Supports(string Language);

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		void Register(MarkdownDocument Document);
	}
}
