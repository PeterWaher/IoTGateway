using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Abstract base class for block elements with one child.
	/// </summary>
	public abstract class BlockElementSingleChild : MarkdownElementSingleChild
	{
		/// <summary>
		/// Abstract base class for block elements with one child.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Child">Child element.</param>
		public BlockElementSingleChild(MarkdownDocument Document, MarkdownElement Child)
			: base(Document, Child)
		{
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public override bool IsBlockElement => true;
	}
}
