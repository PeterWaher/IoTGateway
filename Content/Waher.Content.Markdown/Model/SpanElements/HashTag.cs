using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
    /// <summary>
    /// Represents a hashtag.
    /// </summary>
    public class HashTag : MarkdownElement
	{
		private readonly string tag;

		/// <summary>
		/// Represents a hashtag.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Tag">Hashtag.</param>
		public HashTag(MarkdownDocument Document, string Tag)
			: base(Document)
		{
			this.tag = Tag;
		}

		/// <summary>
		/// Hashtag
		/// </summary>
		public string Tag => this.tag;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return "#" + this.tag;
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => true;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is HashTag x &&
				this.tag == x.tag &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.tag?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrHashTags++;
		}

	}
}
