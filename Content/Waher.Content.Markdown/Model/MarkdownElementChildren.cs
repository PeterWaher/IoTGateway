using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements with a variable number of child elements.
	/// </summary>
	public abstract class MarkdownElementChildren : MarkdownElement
	{
		private IEnumerable<MarkdownElement> children;

		/// <summary>
		/// Abstract base class for all markdown elements with a variable number of child elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public MarkdownElementChildren(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document)
		{
			this.children = Children;
		}

		/// <summary>
		/// Abstract base class for all markdown elements with a variable number of child elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public MarkdownElementChildren(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document)
		{
			this.children = Children;
		}

		/// <summary>
		/// Adds children to the element.
		/// </summary>
		/// <param name="NewChildren">New children to add.</param>
		public void AddChildren(params MarkdownElement[] NewChildren)
		{
			this.AddChildren((IEnumerable<MarkdownElement>)NewChildren);
		}

		/// <summary>
		/// Adds children to the element.
		/// </summary>
		/// <param name="NewChildren">New children to add.</param>
		public void AddChildren(IEnumerable<MarkdownElement> NewChildren)
		{
			LinkedList<MarkdownElement> Children = this.children as LinkedList<MarkdownElement>;
			if (Children == null)
			{
				Children = new LinkedList<MarkdownElement>();
				foreach (MarkdownElement E in this.children)
					Children.AddLast(E);
				this.children = Children;
			}

			foreach (MarkdownElement E in NewChildren)
				Children.AddLast(E);
		}

		/// <summary>
		/// Child elements.
		/// </summary>
		public IEnumerable<MarkdownElement> Children
		{
			get { return this.children; }
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GeneratePlainText(Output);
		}

		/// <summary>
		/// If elements of this type should be joined over paragraphs.
		/// </summary>
		internal virtual bool JoinOverParagraphs
		{
			get { return false; }
		}
	}
}
