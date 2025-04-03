using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Super-script text
	/// </summary>
	public class SuperScript : MarkdownElementChildren
	{
		/// <summary>
		/// Super-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		public SuperScript(MarkdownDocument Document, ChunkedList<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
		}

		/// <summary>
		/// Super-script text
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Text">Superscript text.</param>
		public SuperScript(MarkdownDocument Document, string Text)
			: base(Document, new MarkdownElement[] { new InlineText(Document, Text) })
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
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new SuperScript(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrSuperScript++;
		}

	}
}
