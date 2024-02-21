using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Invisible break
	/// </summary>
	public class InvisibleBreak : BlockElement
	{
		private readonly string row;

		/// <summary>
		/// Invisible break
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Row">Markdown definition.</param>
		public InvisibleBreak(MarkdownDocument Document, string Row)
			: base(Document)
		{
			this.row = Row;
		}

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
			return E is InvisibleBreak x &&
				x.row == this.row &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is InvisibleBreak x &&
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

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrInvisibleBreaks++;
		}

	}
}
