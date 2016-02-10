using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements with one child element.
	/// </summary>
	public abstract class MarkdownElementSingleChild : MarkdownElement
	{
		private MarkdownElement child;

		/// <summary>
		/// Abstract base class for all markdown elements with one child element.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Child">Child element.</param>
		public MarkdownElementSingleChild(MarkdownDocument Document, MarkdownElement Child)
			: base(Document)
		{
			NestedBlock NestedBlock = Child as NestedBlock;
			if (NestedBlock != null && NestedBlock.HasOneChild)
				this.child = NestedBlock.FirstChild;
			else
				this.child = Child;
		}

		/// <summary>
		/// Child element.
		/// </summary>
		public MarkdownElement Child
		{
			get { return this.child; }
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			if (this.child != null)
				this.child.GeneratePlainText(Output);
		}

	}
}
