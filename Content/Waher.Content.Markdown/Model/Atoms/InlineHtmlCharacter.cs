using Waher.Content.Markdown.Model.SpanElements;

namespace Waher.Content.Markdown.Model.Atoms
{
	/// <summary>
	/// Represents a character in inline HTML.
	/// </summary>
	public sealed class InlineHtmlCharacter : Atom
	{
		/// <summary>
		/// Represents a character in inline HTML.
		/// </summary>
		public InlineHtmlCharacter(MarkdownDocument Document, InlineHTML Source, char Character)
			: base(Document, Source, Character)
		{
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrInlineHtml++;
		}
	}
}
