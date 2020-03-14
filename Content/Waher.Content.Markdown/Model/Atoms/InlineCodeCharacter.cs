using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.Atoms
{
	/// <summary>
	/// Represents a character in inline code.
	/// </summary>
	public sealed class InlineCodeCharacter : Atom
	{
		/// <summary>
		/// Represents a character in inline code.
		/// </summary>
		public InlineCodeCharacter(MarkdownDocument Document, InlineCode Source, char Character)
			: base(Document, Source, Character)
		{
		}
	}
}
