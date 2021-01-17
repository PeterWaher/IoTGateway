using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Abbreviation
	/// </summary>
	public class Abbreviation : MarkdownElementChildren
	{
		private readonly string description;

		/// <summary>
		/// Abbreviation
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Description">Description of abbreviation.</param>
		public Abbreviation(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements, string Description)
			: base(Document, ChildElements)
		{
			this.description = Description;
		}

		/// <summary>
		/// Description
		/// </summary>
		public string Description
		{
			get { return this.description; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('[');
			base.GenerateMarkdown(Output);
			Output.Append("](abbr:");
			Output.Append(this.description);
			Output.Append(')');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<abbr data-title=\"");
			Output.Append(XML.HtmlAttributeEncode(this.description).Replace(" ", "&nbsp;"));
			Output.Append("\">");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</abbr>");
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateXamarinForms(Output, TextAlignment);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Abbreviation");
			Output.WriteAttributeString("description", this.description);
			this.ExportChildren(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Abbreviation(Document, Children, this.description);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Abbreviation x &&
				x.description == this.description &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Abbreviation x &&
				this.description == x.description &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.description?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
