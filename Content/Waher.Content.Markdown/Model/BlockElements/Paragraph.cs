using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a paragraph in a markdown document.
	/// </summary>
	public class Paragraph : BlockElementChildren
	{
		private readonly bool @implicit;

		/// <summary>
		/// Represents a paragraph in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public Paragraph(MarkdownDocument Document, ChunkedList<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.@implicit = false;
		}

		/// <summary>
		/// Represents a paragraph in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		/// <param name="Implicit">If paragraph is implicit or not.</param>
		public Paragraph(MarkdownDocument Document, ChunkedList<MarkdownElement> Children,
			bool Implicit)
			: base(Document, Children)
		{
			this.@implicit = Implicit;
		}

		/// <summary>
		/// If paragraph is implicit or not.
		/// </summary>
		public bool Implicit => this.@implicit;

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
			return new Paragraph(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrParagraph++;
		}

	}
}
