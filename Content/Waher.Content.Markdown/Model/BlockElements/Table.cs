using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a table in a markdown document.
	/// </summary>
	public class Table : BlockElement
	{
		private readonly MarkdownElement[][] headers;
		private readonly MarkdownElement[][] rows;
		private readonly TextAlignment[] alignments;
		private readonly string[] alignmentDefinitions;
		private readonly string caption;
		private readonly string id;
		private readonly int columns;

		/// <summary>
		/// Represents a table in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Columns">Columns in table.</param>
		/// <param name="Headers">Header rows.</param>
		/// <param name="Rows">Data rows.</param>
		/// <param name="Alignments">Column alignments.</param>
		/// <param name="AlignmentDefinitions">How the alignments where defined.</param>
		/// <param name="Caption">Table caption.</param>
		/// <param name="Id">Table ID.</param>
		public Table(MarkdownDocument Document, int Columns, MarkdownElement[][] Headers, MarkdownElement[][] Rows,
			TextAlignment[] Alignments, string[] AlignmentDefinitions, string Caption, string Id)
			: base(Document)
		{
			this.columns = Columns;
			this.headers = Headers;
			this.rows = Rows;
			this.alignments = Alignments;
			this.alignmentDefinitions = AlignmentDefinitions;
			this.caption = Caption;
			this.id = Id;
		}

		/// <summary>
		/// Headers in table.
		/// </summary>
		public MarkdownElement[][] Headers => this.headers;

		/// <summary>
		/// Rows in table.
		/// </summary>
		public MarkdownElement[][] Rows => this.rows;

		/// <summary>
		/// Table cell alignments.
		/// </summary>
		public TextAlignment[] Alignments => this.alignments;

		/// <summary>
		/// Originbal Table cell alignment definitions.
		/// </summary>
		public string[] AlignmentDefinitions => this.alignmentDefinitions;

		/// <summary>
		/// Table caption.
		/// </summary>
		public string Caption => this.caption;

		/// <summary>
		/// ID of table.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Number of columns.
		/// </summary>
		public int Columns => this.columns;

		/// <summary>
		/// Any children of the element.
		/// </summary>
		public override IEnumerable<MarkdownElement> Children
		{
			get
			{
				List<MarkdownElement> Result = new List<MarkdownElement>();

				foreach (MarkdownElement[] Row in this.headers)
					Result.AddRange(Row);

				foreach (MarkdownElement[] Row in this.rows)
					Result.AddRange(Row);

				return Result;
			}
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => false;

		/// <summary>
		/// Loops through all child-elements for the element.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public override bool ForEach(MarkdownElementHandler Callback, object State)
		{
			if (!Callback(this, State))
				return false;

			foreach (MarkdownElement[] Row in this.headers)
			{
				foreach (MarkdownElement E in Row)
				{
					if (!(E is null) && !E.ForEach(Callback, State))
						return false;
				}
			}

			foreach (MarkdownElement[] Row in this.rows)
			{
				foreach (MarkdownElement E in Row)
				{
					if (!(E is null) && !E.ForEach(Callback, State))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Table x &&
				this.caption == x.caption &&
				this.id == x.id &&
				this.columns == x.columns &&
				AreEqual(this.alignments, x.alignments) &&
				AreEqual(this.alignmentDefinitions, x.alignmentDefinitions) &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Table x &&
				this.caption == x.caption &&
				this.id == x.id &&
				this.columns == x.columns &&
				AreEqual(this.alignments, x.alignments) &&
				AreEqual(this.alignmentDefinitions, x.alignmentDefinitions) &&
				AreEqual(this.headers, x.headers) &&
				AreEqual(this.rows, x.rows) &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.caption?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.id?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.columns.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.alignments);

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.alignmentDefinitions);

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.headers);

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.rows);

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		private static bool AreEqual(MarkdownElement[][] Items1, MarkdownElement[][] Items2)
		{
			int i, c = Items1.Length;
			if (Items2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (!AreEqual(Items1[i], Items2[i]))
					return false;
			}

			return true;
		}

		private static int GetHashCode(MarkdownElement[][] Items)
		{
			int h1 = 0;
			int h2;

			foreach (MarkdownElement[] Item in Items)
			{
				h2 = GetHashCode(Item);
				h1 = ((h1 << 5) + h1) ^ h2;
			}

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrTables++;
		}

	}
}
