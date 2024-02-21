using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an HTML entity.
	/// </summary>
	public class HtmlEntity : MarkdownElement
	{
		private readonly string entity;

		/// <summary>
		/// Represents an HTML entity.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Entity">HTML Entity.</param>
		public HtmlEntity(MarkdownDocument Document, string Entity)
			: base(Document)
		{
			this.entity = Entity;
		}

		/// <summary>
		/// HTML Entity
		/// </summary>
		public string Entity => this.entity;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return "&" + this.entity + ";";
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
			return obj is HtmlEntity x &&
				this.entity == x.entity &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.entity?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrHtmlEntities++;
			Statistics.NrHtmlEntitiesTotal++;
		}

	}
}
