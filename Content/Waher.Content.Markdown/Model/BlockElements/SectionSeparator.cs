using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Section Separator
	/// </summary>
	public class SectionSeparator : BlockElement
	{
		private readonly string row;
		private readonly int sectionNr;
		private readonly int nrColumns;

		/// <summary>
		/// Section Separator
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="SectionNr">Section number</param>
		/// <param name="NrColumns">Number of columns in following section.</param>
		/// <param name="Row">Markdown definition.</param>
		public SectionSeparator(MarkdownDocument Document, int SectionNr, int NrColumns, string Row)
			: base(Document)
		{
			this.sectionNr = SectionNr;
			this.nrColumns = NrColumns;
			this.row = Row;
		}

		/// <summary>
		/// Section number.
		/// </summary>
		public int SectionNr => this.sectionNr;

		/// <summary>
		/// Number of columns in following section.
		/// </summary>
		public int NrColumns => this.nrColumns;

		/// <summary>
		/// Original row
		/// </summary>
		public string Row => this.row;

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
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is SectionSeparator x &&
				this.nrColumns == x.nrColumns &&
				this.sectionNr == x.sectionNr &&
				this.row == x.row &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is SectionSeparator x &&
				this.nrColumns == x.nrColumns &&
				this.sectionNr == x.sectionNr &&
				this.row == x.row &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.row?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.nrColumns.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.sectionNr.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrSectionSeparators++;
		}

	}
}
