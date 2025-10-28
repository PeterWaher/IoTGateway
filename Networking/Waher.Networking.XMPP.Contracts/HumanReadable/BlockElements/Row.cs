using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Table row
	/// </summary>
	public class Row : BlockElement
	{
		private Cell[] cells;

		/// <summary>
		/// Items
		/// </summary>
		public Cell[] Cells
		{
			get => this.cells;
			set
			{
				this.UnregisterParent(this.cells);
				this.cells = value;
				this.RegisterParent(this.cells);
			}
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.cells is null)
				return this;

			bool Found = false;

			foreach (Cell E in this.cells)
			{
				if (E is null)
					return this;

				HumanReadableElement E2 = await E.IsWellDefined();
				if (!(E2 is null))
					return E2;

				Found = true;
			}

			return Found ? null : this;
		}

		/// <summary>
		/// If the row is a header row.
		/// </summary>
		public bool Header
		{
			get
			{
				if (this.Cells is null)
					return false;

				foreach (Cell Cell in this.Cells)
				{
					if (Cell.Header)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<row>");

			foreach (Cell Cell in this.cells)
				Cell.Serialize(Xml);

			Xml.Append("</row>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			return Task.CompletedTask;
			// Do nothing. Markdown generation handled by Table class.
		}

	}
}
