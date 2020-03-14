using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.Atoms
{
	/// <summary>
	/// Represents a character in inline HTML.
	/// </summary>
	public sealed class InlineHtmlCharacter : Atom
	{
		/// <summary>
		/// Represents a character in inline HTML.
		/// </summary>
		public InlineHtmlCharacter(MarkdownDocument Document, InlineHTML Source, char Character)
			: base(Document, Source, Character)
		{
		}
	}
}
