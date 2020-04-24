using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a nested block with no special formatting rules in a markdown document.
	/// </summary>
	public class NestedBlock : BlockElementChildren
	{
		/// <summary>
		/// Represents a nested block with no special formatting rules in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NestedBlock(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a nested block with no special formatting rules in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NestedBlock(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			base.GenerateMarkdown(Output);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);
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
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			if (this.HasOneChild)
				this.FirstChild.GenerateXAML(Output, TextAlignment);
			else
			{
				bool SpanOpen = false;

				foreach (MarkdownElement E in this.Children)
				{
					if (E.InlineSpanElement)
					{
						if (!SpanOpen)
						{
							Output.WriteStartElement("TextBlock");
							Output.WriteAttributeString("TextWrapping", "Wrap");
							if (TextAlignment != TextAlignment.Left)
								Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());
							SpanOpen = true;
						}
					}
					else
					{
						if (SpanOpen)
						{
							Output.WriteEndElement();
							SpanOpen = false;
						}
					}

					E.GenerateXAML(Output, TextAlignment);
				}

				if (SpanOpen)
					Output.WriteEndElement();
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get
			{
				if (this.HasOneChild)
					return this.FirstChild.InlineSpanElement;
				else
					return false;
			}
		}

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal override void GetMargins(out int TopMargin, out int BottomMargin)
		{
			bool First = true;

			TopMargin = BottomMargin = 0;

			foreach (MarkdownElement E in this.Children)
			{
				if (First)
				{
					First = false;
					E.GetMargins(out TopMargin, out BottomMargin);
				}
				else
					E.GetMargins(out int _, out BottomMargin);
			}
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "NestedBlock");
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
			return new NestedBlock(Document, Children);
		}

	}
}
