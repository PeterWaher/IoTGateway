using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Footnote
	/// </summary>
	public class Footnote : BlockElementChildren
	{
		private readonly string key;
		private bool referenced;
		private bool tableCellContents;
		private bool backlinkAdded;

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Children">Child elements.</param>
		public Footnote(MarkdownDocument Document, string Key, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Children">Child elements.</param>
		public Footnote(MarkdownDocument Document, string Key, params MarkdownElement[] Children)
			: base(Document, Children)
		{
			this.key = Key;
		}

		/// <summary>
		/// Footnote key
		/// </summary>
		public string Key => this.key;

		/// <summary>
		/// If the Footnote has been referenced during rendering, and therefore needs
		/// to be shown at the end of the rendering process.
		/// </summary>
		public bool Referenced
		{
			get => this.referenced;
			set => this.referenced = value;
		}

		/// <summary>
		/// If the Footnote defines a table cell.
		/// </summary>
		internal bool TableCellContents
		{
			get => this.tableCellContents;
			set => this.tableCellContents = value;
		}

		/// <summary>
		/// If a backlink has been added to the footnote.
		/// </summary>
		public bool BacklinkAdded
		{
			get => this.backlinkAdded;
			set => this.backlinkAdded = value;
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
				if (this.HasOneChild)
					return this.FirstChild.InlineSpanElement;
				else
					return false;
			}
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Footnote(Document, this.key, Children);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Footnote x &&
				x.key == this.key &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Footnote x &&
				this.key == x.key &&
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

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrFootnotes++;
		}

	}
}
