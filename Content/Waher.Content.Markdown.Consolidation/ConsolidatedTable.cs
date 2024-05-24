using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Rendering;

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
		private readonly Dictionary<long, TextAlignment?> headerAlignments = new Dictionary<long, TextAlignment?>();
		private readonly Dictionary<long, TextAlignment?> cellAlignments = new Dictionary<long, TextAlignment?>();
		private readonly Dictionary<int, TextAlignment> columnAlignments = new Dictionary<int, TextAlignment>();
		private readonly Dictionary<int, string> sources = new Dictionary<int, string>();
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);

		/// <summary>
		/// Represents a consolidated table.
		/// </summary>
		/// <param name="MarkdownTable">Markdown table.</param>
		private ConsolidatedTable(Table MarkdownTable)
		{
			this.nrColumns = MarkdownTable.Columns;
			this.nrHeaderRows = MarkdownTable.Headers.Length;
			this.nrCellRows = MarkdownTable.Rows.Length;
			this.caption = MarkdownTable.Caption;
			this.id = MarkdownTable.Id;
		}

		/// <summary>
		/// Creates a consolidated table.
		/// </summary>
		/// <param name="Source">Source of table.</param>
		/// <param name="MarkdownTable">Markdown table.</param>
		/// <returns>Consolidated table instance.</returns>
		public static async Task<ConsolidatedTable> CreateAsync(string Source, Table MarkdownTable)
		{
			ConsolidatedTable Result = new ConsolidatedTable(MarkdownTable);
			MarkdownElement[] Row;
			TextAlignment?[] CellAlignments;
			int RowIndex, NrRows;
			int ColumnIndex, NrColumns = MarkdownTable.Columns;
			long l;

			for (ColumnIndex = 0; ColumnIndex < NrColumns; ColumnIndex++)
			{
				Result.columnIndices[await Result.GetHeaderKey(MarkdownTable.Headers, ColumnIndex)] = ColumnIndex;
				Result.columnAlignments[ColumnIndex] = MarkdownTable.ColumnAlignments[ColumnIndex];
			}

			for (RowIndex = 0, NrRows = MarkdownTable.Headers.Length; RowIndex < NrRows; RowIndex++)
			{
				Row = MarkdownTable.Headers[RowIndex];
				CellAlignments = MarkdownTable.HeaderCellAlignments[RowIndex];

				for (ColumnIndex = 0; ColumnIndex < NrColumns; ColumnIndex++)
				{
					l = (((long)RowIndex) << 32) + ColumnIndex;
					Result.headers[l] = Row[ColumnIndex];
					Result.headerAlignments[l] = CellAlignments[ColumnIndex];
				}
			}

			for (RowIndex = 0, NrRows = MarkdownTable.Headers.Length; RowIndex < NrRows; RowIndex++)
			{
				Row = MarkdownTable.Rows[RowIndex];
				CellAlignments = MarkdownTable.RowCellAlignments[RowIndex];

				for (ColumnIndex = 0; ColumnIndex < NrColumns; ColumnIndex++)
				{
					l = (((long)RowIndex) << 32) + ColumnIndex;
					Result.cells[l] = Row[ColumnIndex];
					Result.cellAlignments[l] = CellAlignments[ColumnIndex];
				}

				Result.sources[ColumnIndex] = Source;
			}

			return Result;
		}

		private async Task<string> GetHeaderKey(MarkdownElement[][] Headers, int ColumnIndex)
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
					await Row[ColumnIndex].GenerateMarkdown(sb);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Number of columns.
		/// </summary>
		public Task<int> NrColumns => this.GetNrColumns();

		/// <summary>
		/// Gets the number of columns.
		/// </summary>
		/// <returns>Number of columns.</returns>
		public async Task<int> GetNrColumns()
		{
			await this.synchObj.WaitAsync();
			try
			{
				return this.nrColumns;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <summary>
		/// Number of header rows.
		/// </summary>
		public Task<int> NrHeaderRows => this.GetNrHeaderRows();

		/// <summary>
		/// Gets the number of header rows.
		/// </summary>
		/// <returns>Number of header rows.</returns>
		public async Task<int> GetNrHeaderRows()
		{
			await this.synchObj.WaitAsync();
			try
			{
				return this.nrHeaderRows;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <summary>
		/// Number of cell rows.
		/// </summary>
		public Task<int> NrCellRows => this.GetNrCellRows();

		/// <summary>
		/// Gets the number of cell rows.
		/// </summary>
		/// <returns>Number of cell rows.</returns>
		public async Task<int> GetNrCellRows()
		{
			await this.synchObj.WaitAsync();
			try
			{
				return this.nrCellRows;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <summary>
		/// Gets the alignment of a column.
		/// </summary>
		/// <param name="Column">Zero-based column index.</param>
		/// <returns>Column alignment.</returns>
		public async Task<TextAlignment> GetAlignment(int Column)
		{
			await this.synchObj.WaitAsync();
			try
			{
				if (Column < 0 || Column > this.nrColumns)
					throw new ArgumentOutOfRangeException("Invalid column.", nameof(Column));

				return this.columnAlignments.TryGetValue(Column, out TextAlignment Alignment) ? Alignment : TextAlignment.Left;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <summary>
		/// Adds data from a Markdown table to the consolidated table.
		/// </summary>
		/// <param name="Source">Source of table.</param>
		/// <param name="MarkdownTable">Table to add</param>
		public async Task Add(string Source, Table MarkdownTable)
		{
			await this.synchObj.WaitAsync();
			try
			{
				if (this.caption != MarkdownTable.Caption)
					this.caption = string.Empty;

				if (this.id != MarkdownTable.Id)
					this.id = string.Empty;

				if (MarkdownTable.Headers.Length > this.nrHeaderRows)
					this.nrHeaderRows = MarkdownTable.Headers.Length;

				MarkdownElement[] Row;
				TextAlignment?[] CellAlignments;
				int[] Indices = new int[MarkdownTable.Columns];
				int i, i2, j, j2;
				int RowIndex, NrRows;
				long l;
				string Key;

				i = 0;
				foreach (TextAlignment Alignment in MarkdownTable.ColumnAlignments)
				{
					Key = await this.GetHeaderKey(MarkdownTable.Headers, i);

					if (!this.columnIndices.TryGetValue(Key, out i2))
					{
						i2 = this.nrColumns++;
						this.columnIndices[Key] = i2;
						this.columnAlignments[i2] = Alignment;

						for (RowIndex = 0, NrRows = MarkdownTable.Headers.Length; RowIndex < NrRows; RowIndex++)
						{
							Row = MarkdownTable.Headers[RowIndex];
							CellAlignments = MarkdownTable.HeaderCellAlignments[RowIndex];

							l = (((long)RowIndex) << 32) + i2;
							this.headers[l] = Row[i];
							this.headerAlignments[l] = CellAlignments[i];
						}
					}

					Indices[i++] = i2;
				}

				i = 0;
				i2 = this.nrCellRows;

				this.nrCellRows += MarkdownTable.Rows.Length;

				for (RowIndex = 0, NrRows = MarkdownTable.Rows.Length; RowIndex < NrRows; RowIndex++)
				{
					Row = MarkdownTable.Rows[RowIndex];
					CellAlignments = MarkdownTable.RowCellAlignments[RowIndex];

					j = 0;
					foreach (MarkdownElement Element in Row)
					{
						j2 = Indices[j];
						l = (((long)i2) << 32) + j2;
						this.cells[l] = Element;
						this.cellAlignments[l] = CellAlignments[j];
						j++;
					}

					i++;
					this.sources[i2++] = Source;
				}
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <summary>
		/// Exports the consolidated table to Markdown.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		public async Task Export(StringBuilder Markdown)
		{
			await this.synchObj.WaitAsync();
			try
			{
				TextAlignment? CellAlignment;
				long l;
				int i, j;

				for (i = 0; i < this.nrHeaderRows; i++)
				{
					Markdown.Append("| Nr | Source |");

					for (j = 0; j < this.nrColumns; j++)
					{
						l = (((long)i) << 32) + j;

						if (!this.headerAlignments.TryGetValue(l, out CellAlignment))
							CellAlignment = null;

						if (this.headers.TryGetValue(l, out MarkdownElement E))
						{
							if (E is null)
								Markdown.Append('|');
							else
							{
								Markdown.Append(' ');

								if (CellAlignment.HasValue)
								{
									switch (CellAlignment.Value)
									{
										case TextAlignment.Left:
											Markdown.Append("<<");
											await E.GenerateMarkdown(Markdown);
											break;

										case TextAlignment.Center:
											Markdown.Append(">>");
											await E.GenerateMarkdown(Markdown);
											Markdown.Append("<<");
											break;

										case TextAlignment.Right:
											await E.GenerateMarkdown(Markdown);
											Markdown.Append(">>");
											break;

										default:
											await E.GenerateMarkdown(Markdown);
											break;
									}
								}
								else
									await E.GenerateMarkdown(Markdown);

								Markdown.Append(" |");
							}
						}
						else
							Markdown.Append(" |");
					}

					Markdown.AppendLine();
				}

				Markdown.Append("|--:|:--|");

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
					Markdown.Append("| ");
					Markdown.Append((i + 1).ToString());
					Markdown.Append(" | `");

					if (this.sources.TryGetValue(i, out string Source))
						Markdown.Append(Source);

					Markdown.Append("` |");

					for (j = 0; j < this.nrColumns; j++)
					{
						l = (((long)i) << 32) + j;

						if (!this.cellAlignments.TryGetValue(l, out CellAlignment))
							CellAlignment = null;

						if (this.cells.TryGetValue(l, out MarkdownElement E))
						{
							if (E is null)
								Markdown.Append('|');
							else
							{
								Markdown.Append(' ');

								if (CellAlignment.HasValue)
								{
									switch (CellAlignment.Value)
									{
										case TextAlignment.Left:
											Markdown.Append("<<");
											await E.GenerateMarkdown(Markdown);
											break;

										case TextAlignment.Center:
											Markdown.Append(">>");
											await E.GenerateMarkdown(Markdown);
											Markdown.Append("<<");
											break;

										case TextAlignment.Right:
											await E.GenerateMarkdown(Markdown);
											Markdown.Append(">>");
											break;

										default:
											await E.GenerateMarkdown(Markdown);
											break;
									}
								}
								else
									await E.GenerateMarkdown(Markdown);

								Markdown.Append(" |");
							}
						}
						else
							Markdown.Append(" |");
					}

					Markdown.AppendLine();
				}
			}
			finally
			{
				this.synchObj.Release();
			}
		}

	}
}
