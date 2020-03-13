using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia reference
	/// </summary>
	public class MultimediaReference : LinkReference
	{
		private readonly bool aloneInParagraph;

		/// <summary>
		/// Multimedia reference.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Multimedia label.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		public MultimediaReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label, bool AloneInParagraph)
			: base(Document, ChildElements, Label)
		{
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph
		{
			get { return this.aloneInParagraph; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('!');
			base.GenerateMarkdown(Output);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.Label);

			if (Multimedia != null)
				Multimedia.MultimediaHandler.GenerateHTML(Output, Multimedia.Items, this.Children, this.aloneInParagraph, this.Document);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			base.GeneratePlainText(Output);

			if (this.aloneInParagraph)
			{
				Output.AppendLine();
				Output.AppendLine();
			}
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			Multimedia Multimedia = this.Document.GetReference(this.Label);

			if (Multimedia != null)
			{
				Multimedia.MultimediaHandler.GenerateXAML(Output, Settings, TextAlignment, Multimedia.Items, this.Children, 
					this.aloneInParagraph, this.Document);
			}
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph
		{
			get { return true; }
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is MultimediaReference x &&
				this.aloneInParagraph == x.aloneInParagraph &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.aloneInParagraph.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
