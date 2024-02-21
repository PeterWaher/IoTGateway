using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an HTML entity in Unicode format.
	/// </summary>
	public class HtmlEntityUnicode : MarkdownElement
	{
		private readonly int code;

		/// <summary>
		/// Represents an HTML entity in Unicode format.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Code">HTML Entity.</param>
		public HtmlEntityUnicode(MarkdownDocument Document, int Code)
			: base(Document)
		{
			this.code = Code;
		}

		/// <summary>
		/// Unicode character
		/// </summary>
		public int Code => this.code;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return new string((char)this.code, 1);
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
			return obj is HtmlEntityUnicode x &&
				this.code == x.code &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.code.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrHtmlUnicodeEntities++;
			Statistics.NrHtmlEntitiesTotal++;
		}

	}
}
