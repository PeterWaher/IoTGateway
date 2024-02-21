using System.Threading.Tasks;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an Emoji.
	/// </summary>
	public class EmojiReference : MarkdownElement
	{
		private readonly EmojiInfo emoji;
		private readonly int level;
		private readonly string delimiter;

		/// <summary>
		/// Represents an Emoji.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Emoji">Emoji reference.</param>
		public EmojiReference(MarkdownDocument Document, EmojiInfo Emoji)
			: this(Document, Emoji, 1)
		{
		}

		/// <summary>
		/// Represents an Emoji.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Emoji">Emoji reference.</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		public EmojiReference(MarkdownDocument Document, EmojiInfo Emoji, int Level)
			: base(Document)
		{
			this.emoji = Emoji;
			this.level = Level;
			this.delimiter = Level == 1 ? ":" : new string(':', Level);
		}

		/// <summary>
		/// Emoji information.
		/// </summary>
		public EmojiInfo Emoji => this.emoji;

		/// <summary>
		/// Level (number of colons used to define the emoji)
		/// </summary>
		public int Level => this.level;

		/// <summary>
		/// Delimiter string used to identify emoji.
		/// </summary>
		public string Delimiter => this.delimiter;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.delimiter + this.emoji.ShortName + this.delimiter;
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		public override bool OutsideParagraph => true;

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
			return obj is EmojiReference x &&
				this.emoji.ShortName == x.emoji.ShortName &&
				this.level == x.level &&
				this.delimiter == x.delimiter &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.emoji.ShortName.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.level.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.delimiter.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrEmojiReference++;
		}

	}
}
