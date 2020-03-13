using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Abstract base class for all markdown elements with one child element.
	/// </summary>
	public abstract class MarkdownElementSingleChild : MarkdownElement
	{
		private readonly MarkdownElement child;

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
			get { return this.child; }
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
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			if (this.child != null)
				this.child.GeneratePlainText(Output);
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

			if (this.child != null && !this.child.ForEach(Callback, State))
				return false;

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
			this.ExportChild(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports the child element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		protected virtual void ExportChild(XmlWriter Output)
		{
			if (this.child != null)
				this.child.Export(Output);
		}

	}
}
