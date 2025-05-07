using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.CodeContent
{
	/// <summary>
	/// Base64-encoded text content.
	/// </summary>
	public class TextContent : ICodeContent
	{
		/// <summary>
		/// Base64-encoded text content.
		/// </summary>
		public TextContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports code content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			if (Language.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// If script is evaluated for this type of code block.
		/// </summary>
		public bool EvaluatesScript => false;

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
		}
	}
}
