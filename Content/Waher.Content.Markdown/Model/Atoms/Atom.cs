using System;
using System.Text;
using System.Xml;

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
		internal override bool InlineSpanElement => true;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			MustBeReassembled();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			MustBeReassembled();
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			MustBeReassembled();
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			MustBeReassembled();
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			MustBeReassembled();
		}

		private static void MustBeReassembled()
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
