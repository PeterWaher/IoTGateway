using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model.Atoms;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline HTML.
	/// </summary>
	public class InlineHTML : MarkdownElement, IEditableText
	{
		private readonly string html;

		/// <summary>
		/// Inline HTML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="HTML">Inline HTML.</param>
		public InlineHTML(MarkdownDocument Document, string HTML)
			: base(Document)
		{
			this.html = HTML;
		}

		/// <summary>
		/// HTML
		/// </summary>
		public string HTML => this.html;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.html;
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
			return obj is InlineHTML x &&
				this.html == x.html &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.html?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Return an enumeration of the editable HTML as atoms.
		/// </summary>
		/// <returns>Atoms.</returns>
		public IEnumerable<Atom> Atomize()
		{
			LinkedList<Atom> Result = new LinkedList<Atom>();

			foreach (char ch in this.html)
				Result.AddLast(new InlineHtmlCharacter(this.Document, this, ch));

			return Result;
		}

		/// <summary>
		/// Assembles a markdown element from a sequence of atoms.
		/// </summary>
		/// <param name="Document">Document that will contain the new element.</param>
		/// <param name="Text">Assembled text.</param>
		/// <returns>Assembled markdown element.</returns>
		public MarkdownElement Assemble(MarkdownDocument Document, string Text)
		{
			return new InlineHTML(Document, Text);
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrInlineHtml++;
		}

	}
}
