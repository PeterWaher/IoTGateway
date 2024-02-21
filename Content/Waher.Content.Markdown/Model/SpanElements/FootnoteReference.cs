using System.Threading.Tasks;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Footnote reference
	/// </summary>
	public class FootnoteReference : MarkdownElement
	{
		private readonly string key;
		private bool autoExpand;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public FootnoteReference(MarkdownDocument Document, string Key)
			: base(Document)
		{
			this.key = Key;
			this.autoExpand = false;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// If the footnote should automatically be expanded when rendered,
		/// if format supports auto-expansion.
		/// </summary>
		public bool AutoExpand
		{
			get => this.autoExpand;
			internal set => this.autoExpand = value;
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.key;
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement
		{
			get
			{
				if (this.autoExpand && (this.Document?.TryGetFootnote(this.key, out Footnote Footnote) ?? false))
					return Footnote.InlineSpanElement;
				else
					return true;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is FootnoteReference x &&
				this.key == x.key &&
				this.autoExpand == x.autoExpand &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.key?.GetHashCode() ?? 0;
			int h3 = this.autoExpand.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h1 = ((h1 << 5) + h1) ^ h3;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrFootnoteReference++;
		}
	}
}
