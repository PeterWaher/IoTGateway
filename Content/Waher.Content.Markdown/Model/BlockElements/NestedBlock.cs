using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a nested block with no special formatting rules in a markdown document.
	/// </summary>
	public class NestedBlock : BlockElementChildren
	{
		private readonly bool hasBlocks = false;

		/// <summary>
		/// Represents a nested block with no special formatting rules in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NestedBlock(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.hasBlocks = this.CalcHasBlocks();
		}

		/// <summary>
		/// Represents a nested block with no special formatting rules in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NestedBlock(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
			this.hasBlocks = this.CalcHasBlocks();
		}

		private bool CalcHasBlocks()
		{
			bool HasBlocks = false;
			bool HasSpans = false;
			bool Inconsistent = false;

			foreach (MarkdownElement E in this.Children)
			{
				if (E.IsBlockElement)
				{
					HasBlocks = true;
					if (HasSpans)
					{
						Inconsistent = true;
						break;
					}
				}
				else
				{
					HasSpans = true;
					if (HasBlocks)
					{
						Inconsistent = true;
						break;
					}
				}
			}

			if (!Inconsistent)
				return HasBlocks;

			LinkedList<MarkdownElement> NewChildren = new LinkedList<MarkdownElement>();
			LinkedList<MarkdownElement> Spans = null;

			foreach (MarkdownElement E in this.Children)
			{
				if (E.IsBlockElement)
				{
					if (!(Spans is null))
					{
						NewChildren.AddLast(new Paragraph(this.Document, Spans));
						Spans = null;
					}

					NewChildren.AddLast(E);
				}
				else
				{
					if (Spans is null)
						Spans = new LinkedList<MarkdownElement>();

					Spans.AddLast(E);
				}
			}

			if (!(Spans is null))
				NewChildren.AddLast(new Paragraph(this.Document, Spans));

			this.SetChildren(NewChildren);

			return true;
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public override bool IsBlockElement => this.hasBlocks;

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GeneratePlainText(Output);
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			if (this.HasOneChild)
				await this.FirstChild.GenerateXAML(Output, TextAlignment);
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

					await E.GenerateXAML(Output, TextAlignment);
				}

				if (SpanOpen)
					Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			if (this.HasOneChild)
				await this.FirstChild.GenerateXamarinForms(Output, State);
			else
			{
				StringBuilder Html = null;

				foreach (MarkdownElement E in this.Children)
				{
					if (E.InlineSpanElement)
					{
						if (Html is null)
							Html = new StringBuilder();

						await E.GenerateHTML(Html);
					}
					else
					{
						if (!(Html is null))
						{
							Output.WriteStartElement("Label");
							Output.WriteAttributeString("LineBreakMode", "WordWrap");
							Header.XamarinFormsLabelAlignment(Output, State);
							Output.WriteAttributeString("TextType", "Html");
							Output.WriteCData(Html.ToString());
							Output.WriteEndElement();

							Html = null;
						}

						await E.GenerateXamarinForms(Output, State);
					}
				}

				if (!(Html is null))
				{
					Output.WriteStartElement("Label");
					Output.WriteAttributeString("LineBreakMode", "WordWrap");
					Header.XamarinFormsLabelAlignment(Output, State);
					Output.WriteAttributeString("TextType", "Html");
					Output.WriteCData(Html.ToString());
					Output.WriteEndElement();
				}
			}
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateLaTeX(Output);
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

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrNestedBlocks++;
		}

	}
}
