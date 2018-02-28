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
	}
}
