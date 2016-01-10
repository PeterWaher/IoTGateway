using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements.
	/// </summary>
	public abstract class MarkdownElement
	{
		private MarkdownDocument document;

		/// <summary>
		/// Abstract base class for all markdown elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public MarkdownElement(MarkdownDocument Document)
		{
			this.document = Document;
		}

		/// <summary>
		/// Markdown document.
		/// </summary>
		public MarkdownDocument Document
		{
			get { return this.document; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public abstract void GenerateHTML(StringBuilder Output);

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public abstract void GeneratePlainText(StringBuilder Output);

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal virtual bool OutsideParagraph
		{
			get { return false; }
		}
	}
}
