using System;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for all markdown handlers of code content that generates an image output.
	/// </summary>
	public interface IImageCodeContent : ICodeContent
	{
		/// <summary>
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		PixelInformation GenerateImage(string[] Rows, string Language, MarkdownDocument Document);
	}
}
