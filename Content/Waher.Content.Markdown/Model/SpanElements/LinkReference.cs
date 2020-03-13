using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link reference
	/// </summary>
	public class LinkReference : MarkdownElementChildren
	{
		private readonly string label;

		/// <summary>
		/// Link reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Link label.</param>
		public LinkReference(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements, string Label)
			: base(Document, ChildElements)
		{
			this.label = Label;
		}

		/// <summary>
		/// Link label
		/// </summary>
		public string Label
		{
			get { return this.label; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('[');
			base.GenerateMarkdown(Output);
			Output.Append("][");
			Output.Append(this.label);
			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (Multimedia != null)
				Link.GenerateHTML(Output, Multimedia.Items[0].Url, Multimedia.Items[0].Title, this.Children, this.Document);
			else
			{
				foreach (MarkdownElement E in this.Children)
					E.GenerateHTML(Output);
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.label;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (Multimedia != null)
			{
				Link.GenerateXAML(Output, Settings, TextAlignment, Multimedia.Items[0].Url, Multimedia.Items[0].Title, this.Children,
					this.Document);
			}
			else
			{
				foreach (MarkdownElement E in this.Children)
					E.GenerateXAML(Output, Settings, TextAlignment);
			}
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
			Output.WriteStartElement("LinkReference");
			Output.WriteAttributeString("label", this.label);
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
			return new LinkReference(Document, Children, this.label);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is LinkReference x &&
				x.label == this.label &&
				base.SameMetaData(E);
		}

	}
}
