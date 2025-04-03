using System.Collections.Generic;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements with a variable number of child elements.
	/// </summary>
	public abstract class MarkdownElementChildren : MarkdownElement
	{
		private ChunkedList<MarkdownElement> children;

		/// <summary>
		/// Abstract base class for all markdown elements with a variable number of child elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public MarkdownElementChildren(MarkdownDocument Document, ChunkedList<MarkdownElement> Children)
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
			this.children = new ChunkedList<MarkdownElement>(Children);
		}

		/// <summary>
		/// Adds a child to the element.
		/// </summary>
		/// <param name="NewChild">New child to add.</param>
		public virtual void AddChild(MarkdownElement NewChild)
		{
			this.children.Add(NewChild);
		}

		/// <summary>
		/// Adds children to the element.
		/// </summary>
		/// <param name="NewChildren">New children to add.</param>
		public virtual void AddChildren(ChunkedList<MarkdownElement> NewChildren)
		{
			this.children.AddRange(NewChildren);
		}

		/// <summary>
		/// If there is a first item.
		/// </summary>
		public bool HasFirstChild => this.children.HasFirstItem;

		/// <summary>
		/// First child, or null if none.
		/// </summary>
		public MarkdownElement FirstChild => this.children.FirstItem;

		/// <summary>
		/// If there is a last item.
		/// </summary>
		public bool HasLastChild => this.children.HasLastItem;

		/// <summary>
		/// Last child, or null if none.
		/// </summary>
		public MarkdownElement LastChild => this.children.LastItem;

		/// <summary>
		/// If the element has only one child.
		/// </summary>
		public bool HasOneChild => this.children.Count == 1;

		/// <summary>
		/// Any children of the element.
		/// </summary>
		public override ChunkedList<MarkdownElement> Children => this.children;

		/// <summary>
		/// Sets the children of the node.
		/// </summary>
		/// <param name="Children">Children to set.</param>
		protected void SetChildren(ChunkedList<MarkdownElement> Children)
		{
			this.children = Children;
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public abstract MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document);

		/// <summary>
		/// If elements of this type should be joined over paragraphs.
		/// </summary>
		internal virtual bool JoinOverParagraphs => false;

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

			if (!(this.children is null))
			{
				foreach (MarkdownElement E in this.children)
				{
					if (!E.ForEach(Callback, State))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is MarkdownElementChildren x) || !base.Equals(obj))
				return false;

			IEnumerator<MarkdownElement> e1 = this.children.GetEnumerator();
			IEnumerator<MarkdownElement> e2 = x.children.GetEnumerator();
			bool b1, b2;

			while (true)
			{
				b1 = e1.MoveNext();
				b2 = e2.MoveNext();

				if (b1 ^ b2)
					return false;

				if (!b1)
					return true;

				if (!e1.Current.Equals(e2.Current))
					return false;
			}
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2;

			foreach (MarkdownElement E in this.children)
			{
				h2 = E.GetHashCode();
				h1 = ((h1 << 5) + h1) ^ h2;
			}

			return h1;
		}

	}
}
