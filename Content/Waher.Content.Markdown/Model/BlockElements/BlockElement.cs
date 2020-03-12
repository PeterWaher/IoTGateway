using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Abstract base class for block elements.
	/// </summary>
	public abstract class BlockElement : MarkdownElement
	{
		/// <summary>
		/// Abstract base class for block elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public BlockElement(MarkdownDocument Document)
			: base(Document)
		{
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public override bool IsBlockElement => true;
	}
}
