using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Line break
	/// </summary>
	public class LineBreak : MarkdownElement
	{
		/// <summary>
		/// Line break
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public LineBreak(MarkdownDocument Document)
			: base(Document)
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
		public override bool InlineSpanElement => true;

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrLineBreak++;
		}
	}
}
