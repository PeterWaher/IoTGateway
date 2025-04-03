using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link
	/// </summary>
	public class Link : MarkdownElementChildren
	{
		private readonly string url;
		private readonly string title;

		/// <summary>
		/// Link
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		public Link(MarkdownDocument Document, ChunkedList<MarkdownElement> ChildElements, string Url, string Title)
			: base(Document, ChildElements)
		{
			this.url = Url;
			this.title = Title;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string Url => this.url;

		/// <summary>
		/// Optional Link title.
		/// </summary>
		public string Title => this.title;

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
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Link(Document, Children, this.url, this.title);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Link x &&
				x.url == this.url &&
				x.title == this.title &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Link x &&
				this.url == x.url &&
				this.title == x.title &&
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
			h2 = this.title?.GetHashCode() ?? 0;

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

			Statistics.NrHyperLinks++;
			Statistics.NrUrlHyperLinks++;
		}

	}
}
