using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Represents a consolidated table.
	/// </summary>
	public class ConsolidatedTable
	{
		private string caption;
		private string id;
		private int nrColumns;
		private int nrHeaderRows;
		private int nrCellRows;
		private readonly Dictionary<string, int> columnIndices = new Dictionary<string, int>();
		private readonly Dictionary<long, MarkdownElement> headers = new Dictionary<long, MarkdownElement>();
		private readonly Dictionary<long, MarkdownElement> cells = new Dictionary<long, MarkdownElement>();
		private readonly Dictionary<int, TextAlignment> columnAlignments = new Dictionary<int, TextAlignment>();
		private readonly Dictionary<int, string> sources = new Dictionary<int, string>();
		private readonly object synchObj = new object();

		/// <summary>
		/// Represents a consolidated table.
		/// </summary>
		/// <param name="Source">Source of table.</param>
		/// <param name="MarkdownTable">Markdown table.</param>
		public ConsolidatedTable(string Source, Table MarkdownTable)
		{
			int i, j;

			this.nrColumns = MarkdownTable.Columns;
			this.nrHeaderRows = MarkdownTable.Headers.Length;
			this.nrCellRows = MarkdownTable.Rows.Length;
			this.caption = MarkdownTable.Caption;
			this.id = MarkdownTable.Id;

			i = 0;
			foreach (TextAlignment Alignment in MarkdownTable.Alignments)
			{
				this.columnIndices[this.GetHeaderKey(MarkdownTable.Headers, i)] = i;
				this.columnAlignments[i++] = Alignment;
			}

			i = 0;
			foreach (MarkdownElement[] Row in MarkdownTable.Headers)
			{
				j = 0;
				foreach (MarkdownElement Element in Row)
					this.headers[(((long)i) << 32) + j++] = Element;

				i++;
			}

			i = 0;
			foreach (MarkdownElement[] Row in MarkdownTable.Rows)
			{
				j = 0;
				foreach (MarkdownElement Element in Row)
					this.cells[(((long)i) << 32) + j++] = Element;

				this.sources[i++] = Source;
			}
		}

		private string GetHeaderKey(MarkdownElement[][] Headers, int ColumnIndex)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (MarkdownElement[] Row in Headers)
			{
				if (First)
					First = false;
				else
					sb.AppendLine();

				if (ColumnIndex < Row.Length)
					Row[ColumnIndex].GenerateMarkdown(sb);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Number of columns.
		/// </summary>
		public int NrColumns
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrColumns;
				}
			}
		}

		/// <summary>
		/// Number of header rows.
		/// </summary>
		public int NrHeaderRows
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrHeaderRows;
				}
			}
		}

		/// <summary>
		/// Number of cell rows.
		/// </summary>
		public int NrCellRows
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrCellRows;
				}
			}
		}

		/// <summary>
		/// Gets the alignment of a column.
		/// </summary>
		/// <param name="Column">Zero-based column index.</param>
		/// <returns>Column alignment.</returns>
		public TextAlignment GetAlignment(int Column)
		{
			lock (this.synchObj)
			{
				if (Column < 0 || Column > this.nrColumns)
					throw new ArgumentOutOfRangeException("Invalid column.", nameof(Column));

				return this.columnAlignments.TryGetValue(Column, out TextAlignment Alignment) ? Alignment : TextAlignment.Left;
			}
		}

		/// <summary>
		/// Adds data from a Markdown table to the consolidated table.
		/// </summary>
		/// <param name="Source">Source of table.</param>
		/// <param name="MarkdownTable">Table to add</param>
		public void Add(string Source, Table MarkdownTable)
		{
			lock (this.synchObj)
			{
				if (this.caption != MarkdownTable.Caption)
					this.caption = string.Empty;

				if (this.id != MarkdownTable.Id)
					this.id = string.Empty;

				if (MarkdownTable.Headers.Length > this.nrHeaderRows)
					this.nrHeaderRows = MarkdownTable.Headers.Length;

				int[] Indices = new int[MarkdownTable.Columns];
				int i, i2, j, j2;
				string Key;

				i = 0;
				foreach (TextAlignment Alignment in MarkdownTable.Alignments)
				{
					Key = this.GetHeaderKey(MarkdownTable.Headers, i);

					if (!this.columnIndices.TryGetValue(Key, out i2))
					{
						i2 = this.nrColumns++;
						this.columnIndices[Key] = i2;
						this.columnAlignments[i2] = Alignment;

						j = 0;
						foreach (MarkdownElement[] Row in MarkdownTable.Headers)
							this.headers[(((long)j++) << 32) + i2] = Row[i];
					}

					Indices[i++] = i2;
				}

				i = 0;
				i2 = this.nrCellRows;

				this.nrCellRows += MarkdownTable.Rows.Length;

				foreach (MarkdownElement[] Row in MarkdownTable.Rows)
				{
					j = 0;
					foreach (MarkdownElement Element in Row)
					{
						j2 = Indices[j++];
						this.cells[(((long)i2) << 32) + j2] = Element;
					}

					i++;
					this.sources[i2++] = Source;
				}
			}
		}

		/// <summary>
		/// Exports the consolidated table to Markdown.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		public void Export(StringBuilder Markdown)
		{
			lock (this.synchObj)
			{
				int i, j;

				for (i = 0; i < this.nrHeaderRows; i++)
				{
					Markdown.Append("| Source |");

					for (j = 0; j < this.nrColumns; j++)
					{
						if (this.headers.TryGetValue((((long)i) << 32) + j, out MarkdownElement E))
						{
							if (E is null)
								Markdown.Append('|');
							else
							{
								Markdown.Append(' ');
								E.GenerateMarkdown(Markdown);
								Markdown.Append(" |");
							}
						}
						else
							Markdown.Append(" |");
					}

					Markdown.AppendLine();
				}

				Markdown.Append("|:--|");

				for (j = 0; j < this.nrColumns; j++)
				{
					if (!this.columnAlignments.TryGetValue(j, out TextAlignment Alignment))
						Alignment = TextAlignment.Left;

					switch (Alignment)
					{
						case TextAlignment.Left:
							Markdown.Append(":--|");
							break;

						case TextAlignment.Right:
							Markdown.Append("--:|");
							break;

						case TextAlignment.Center:
							Markdown.Append(":-:|");
							break;

						default:
							Markdown.Append("---|");
							break;
					}
				}

				Markdown.AppendLine();

				for (i = 0; i < this.nrCellRows; i++)
				{
					Markdown.Append("| `");

					if (this.sources.TryGetValue(i, out string Source))
						Markdown.Append(Source);

					Markdown.Append("` |");

					for (j = 0; j < this.nrColumns; j++)
					{
						if (this.cells.TryGetValue((((long)i) << 32) + j, out MarkdownElement E))
						{
							if (E is null)
								Markdown.Append('|');
							else
							{
								Markdown.Append(' ');
								E.GenerateMarkdown(Markdown);
								Markdown.Append(" |");
							}
						}
						else
							Markdown.Append(" |");
					}

					Markdown.AppendLine();
				}
			}
		}

	}
}
