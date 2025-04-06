using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Automatic Link (URL)
	/// </summary>
	public class AutomaticLinkUrl : MarkdownElement
	{
		private readonly string url;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="URL">Automatic URL link.</param>
		public AutomaticLinkUrl(MarkdownDocument Document, string URL)
			: base(Document)
		{
			this.url = URL;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string URL => this.url;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.url;
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
			return obj is AutomaticLinkUrl x &&
				this.url == x.url &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.url?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			if (Statistics.IntUrlHyperlinks is null)
				Statistics.IntUrlHyperlinks = new ChunkedList<string>();

			if (!Statistics.IntUrlHyperlinks.Contains(this.url))
				Statistics.IntUrlHyperlinks.Add(this.url);

			Statistics.NrUrlHyperLinks++;
			Statistics.NrHyperLinks++;
		}

	}
}
