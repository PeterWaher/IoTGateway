using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;
using Waher.Script.Constants;

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
		public NestedBlock(MarkdownDocument Document, ChunkedList<MarkdownElement> Children)
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
			ChunkNode<MarkdownElement> Loop = this.Children.FirstChunk;
			bool HasBlocks = false;
			bool HasSpans = false;
			bool Inconsistent = false;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					if (Loop[i].IsBlockElement)
					{
						HasBlocks = true;
						if (HasSpans)
						{
							Inconsistent = true;
							Loop = null;
							break;
						}
					}
					else
					{
						HasSpans = true;
						if (HasBlocks)
						{
							Inconsistent = true;
							Loop = null;
							break;
						}
					}
				}

				Loop = Loop?.Next;
			}

			if (!Inconsistent)
				return HasBlocks;

			ChunkedList<MarkdownElement> NewChildren = new ChunkedList<MarkdownElement>();
			ChunkedList<MarkdownElement> Spans = null;
			MarkdownElement E;

			Loop = this.Children.FirstChunk;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					E = Loop[i];

					if (E.IsBlockElement)
					{
						if (!(Spans is null))
						{
							NewChildren.Add(new Paragraph(this.Document, Spans, true));
							Spans = null;
						}

						NewChildren.Add(E);
					}
					else
					{
						if (Spans is null)
							Spans = new ChunkedList<MarkdownElement>();

						Spans.Add(E);
					}
				}

				Loop = Loop?.Next;
			}

			if (!(Spans is null))
				NewChildren.Add(new Paragraph(this.Document, Spans, true));

			this.SetChildren(NewChildren);

			return true;
		}

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public override bool IsBlockElement => this.hasBlocks;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement
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
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
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
