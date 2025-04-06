using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Definition descriptions
	/// </summary>
	public class DefinitionDescriptions : BlockElementChildren
	{
		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, ChunkedList<MarkdownElement> Descriptions)
			: base(Document, Descriptions)
		{
		}

		/// <summary>
		/// Definition descriptions
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Descriptions">Descriptions</param>
		public DefinitionDescriptions(MarkdownDocument Document, params MarkdownElement[] Descriptions)
			: base(Document, Descriptions)
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
			return new DefinitionDescriptions(Document, Children);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrDefinitionDescriptions++;
		}

	}
}
