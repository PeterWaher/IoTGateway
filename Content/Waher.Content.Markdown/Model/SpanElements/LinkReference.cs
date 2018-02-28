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
		private string label;

		/// <summary>
		/// Link reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Link label.</param>
		public LinkReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label)
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
	}
}
