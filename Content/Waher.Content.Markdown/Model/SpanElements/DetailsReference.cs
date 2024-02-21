using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Details reference
	/// </summary>
	public class DetailsReference : MetaReference
	{
		/// <summary>
		/// Meta-data reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public DetailsReference(MarkdownDocument Document, string Key)
			: base(Document, Key)
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
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		public override bool OutsideParagraph => true;

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrDetailsReference++;
		}
	}
}
