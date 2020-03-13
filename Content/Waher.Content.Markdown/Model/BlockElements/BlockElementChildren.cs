using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Abstract base class for block elements with children.
	/// </summary>
	public abstract class BlockElementChildren : MarkdownElementChildren
	{
		/// <summary>
		/// Abstract base class for block elements with children.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public BlockElementChildren(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Abstract base class for block elements with children.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public BlockElementChildren(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public override bool IsBlockElement => true;

	}
}
