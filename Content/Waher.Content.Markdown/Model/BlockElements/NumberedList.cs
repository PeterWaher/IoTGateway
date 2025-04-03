using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a numbered list in a markdown document.
	/// </summary>
	public class NumberedList : BlockElementChildren
	{
		/// <summary>
		/// Represents a numbered list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NumberedList(MarkdownDocument Document, ChunkedList<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a numbered list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NumberedList(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => false;

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new NumberedList(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrNumberedLists++;
			Statistics.NrLists++;
		}

		/// <summary>
		/// Adds a child to the element.
		/// </summary>
		/// <param name="NewChild">New child to add.</param>
		public override void AddChild(MarkdownElement NewChild)
		{
			if (this.HasLastChild && 
				this.LastChild is NumberedItem LastItem &&
				NewChild is NumberedItem NewItem)
			{
				if (!NewItem.NumberExplicit)
					NewItem.Number = LastItem.Number + 1;
			}
				
			base.AddChild(NewChild);
		}

		/// <summary>
		/// Adds children to the element.
		/// </summary>
		/// <param name="NewChildren">New children to add.</param>
		public override void AddChildren(ChunkedList<MarkdownElement> NewChildren)
		{
			ChunkNode<MarkdownElement> Loop = NewChildren.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					this.AddChild(Loop[i]);

				Loop = Loop.Next;
			}
		}
	}
}
