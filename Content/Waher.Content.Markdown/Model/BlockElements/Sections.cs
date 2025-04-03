using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a sequence of sections.
	/// </summary>
	public class Sections : BlockElementChildren
	{
		private readonly string initialRow;
		private readonly int initialNrColumns;

		/// <summary>
		/// Represents a sequence of sections.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="InitialNrColumns">Number of columns in first section.</param>
		/// <param name="InitialRow">Initial section separator row, if provided, null otherwise.</param>
		/// <param name="Children">Child elements.</param>
		public Sections(MarkdownDocument Document, int InitialNrColumns, string InitialRow, ChunkedList<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.initialRow = InitialRow;
			this.initialNrColumns = InitialNrColumns;
		}

		/// <summary>
		/// Number of columns for initial section.
		/// </summary>
		public int InitialNrColumns => this.initialNrColumns;

		/// <summary>
		/// Initial row
		/// </summary>
		public string InitialRow => this.initialRow;

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
			return new Sections(Document, this.initialNrColumns, this.initialRow, Children);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Sections x &&
				x.initialNrColumns	 == this.initialNrColumns &&
				x.initialRow == this.initialRow &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Sections x &&
				this.initialNrColumns == x.initialNrColumns &&
				this.initialRow == x.initialRow &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.initialNrColumns.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.initialRow?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrSections++;
		}

	}
}
