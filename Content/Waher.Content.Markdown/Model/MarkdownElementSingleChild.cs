using Waher.Content.Markdown.Model.BlockElements;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements with one child element.
	/// </summary>
	public abstract class MarkdownElementSingleChild : MarkdownElement
	{
		private ChunkedList<MarkdownElement> children = null;
		private MarkdownElement child;

		/// <summary>
		/// Abstract base class for all markdown elements with one child element.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Child">Child element.</param>
		public MarkdownElementSingleChild(MarkdownDocument Document, MarkdownElement Child)
			: base(Document)
		{
			if (Child is NestedBlock NestedBlock && NestedBlock.HasOneChild)
				this.child = NestedBlock.FirstChild;
			else
				this.child = Child;
		}

		/// <summary>
		/// Child element.
		/// </summary>
		public MarkdownElement Child
		{
			get => this.child;
			internal set => this.child = value;
		}

		/// <summary>
		/// Any children of the element.
		/// </summary>
		public override ChunkedList<MarkdownElement> Children
		{
			get
			{
				if (this.children is null)
					this.children = new ChunkedList<MarkdownElement>(this.child);

				return this.children;
			}
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Child"/>.
		/// </summary>
		/// <param name="Child">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public abstract MarkdownElementSingleChild Create(MarkdownElement Child, MarkdownDocument Document);

		/// <summary>
		/// Loops through all child-elements for the element.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public override bool ForEach(MarkdownElementHandler Callback, object State)
		{
			if (!Callback(this, State))
				return false;

			if (!(this.child is null) && !this.child.ForEach(Callback, State))
				return false;

			return true;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is MarkdownElementSingleChild x &&
				(this.child?.Equals(x.child) ?? x.child is null) &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.child?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
