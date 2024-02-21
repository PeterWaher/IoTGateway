using System;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.Atoms
{
	/// <summary>
	/// Represents an atom of editable text (i.e. typed character).
	/// </summary>
	public abstract class Atom : MarkdownElement
	{
		private readonly IEditableText source;
		private readonly char character;

		/// <summary>
		/// Represents an atom of editable text (i.e. typed character).
		/// </summary>
		public Atom(MarkdownDocument Document, IEditableText Source, char Character)
			: base(Document)
		{
			this.source = Source;
			this.character = Character;
		}

		/// <summary>
		/// Character
		/// </summary>
		public char Charater => this.character;

		/// <summary>
		/// Source
		/// </summary>
		public IEditableText Source => this.source;

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => true;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output)
		{
			throw new NotSupportedException("Atomic elements must be reassembled before being exported or used for output.");
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Atom x &&
				this.character == x.character &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = this.character.GetHashCode();
			int h2 = base.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
