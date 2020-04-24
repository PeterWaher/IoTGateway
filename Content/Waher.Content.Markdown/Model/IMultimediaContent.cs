using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
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

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		void GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document);

		/// <summary>
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		void GeneratePlainText(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document);

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items, 
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document);
	}
}
