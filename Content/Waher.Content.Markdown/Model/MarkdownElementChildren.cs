using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

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
		public virtual void AddChildren(IEnumerable<MarkdownElement> NewChildren)
		{
			if (!(this.children is LinkedList<MarkdownElement> Children))
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
		/// First child, or null if none.
		/// </summary>
		public MarkdownElement FirstChild
		{
			get
			{
				foreach (MarkdownElement E in this.children)
					return E;

				return null;
			}
		}

		/// <summary>
		/// Last child, or null if none.
		/// </summary>
		public MarkdownElement LastChild
		{
			get
			{
				MarkdownElement Result = null;

				foreach (MarkdownElement E in this.children)
					Result = E;

				return Result;
			}
		}

		/// <summary>
		/// If the element has only one child.
		/// </summary>
		public bool HasOneChild
		{
			get
			{
				bool First = true;

				foreach (MarkdownElement E in this.children)
				{
					if (First)
						First = false;
					else
						return false;
				}

				return !First;
			}
		}

		/// <summary>
		/// Child elements.
		/// </summary>
		public IEnumerable<MarkdownElement> Children
		{
			get { return this.children; }
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public abstract MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document);

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateMarkdown(Output);
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

			if (this.children != null)
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
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="ElementName">Name of element.</param>
		public void Export(XmlWriter Output, string ElementName)
		{
			Output.WriteStartElement(ElementName);
			this.ExportChildren(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports the child elements to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		protected virtual void ExportChildren(XmlWriter Output)
		{
			if (this.children != null)
			{
				foreach (MarkdownElement E in this.children)
					E.Export(Output);
			}
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
