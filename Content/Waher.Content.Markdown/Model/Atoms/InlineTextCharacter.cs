using System;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.Atoms
{
	/// <summary>
	/// Represents a character in inline text.
	/// </summary>
	public sealed class InlineTextCharacter : Atom
	{
		/// <summary>
		/// Represents a character in inline text.
		/// </summary>
		public InlineTextCharacter(MarkdownDocument Document, InlineText Source, char Character)
			: base(Document, Source, Character)
		{
		}
	}
}
