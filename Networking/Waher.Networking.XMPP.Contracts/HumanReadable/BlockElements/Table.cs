using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Table
	/// </summary>
	public class Table : BlockElement
	{
		private Row[] rows;

		/// <summary>
		/// Rows
		/// </summary>
		public Row[] Rows
		{
			get => this.rows;
			set => this.rows = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.rows is null)
				return this;

			bool Found = false;

			foreach (Row E in this.rows)
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
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<table>");

			foreach (Row Row in this.rows)
				Row.Serialize(Xml);

			Xml.Append("</table>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override async Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			LinkedList<KeyValuePair<string, string[]>> Notes = null;
			List<bool> IsRowHeader = new List<bool>();
			List<int[]> ColumnAlignmentCounts = new List<int[]>();
			TextAlignment[] ColumnAlignments;
			int NrRows = 0;
			int NrColumns = 0;

			foreach (Row Row in this.Rows)
			{
				NrRows++;
				IsRowHeader.Add(Row.Header);

				int ColumnNr = 0;

				foreach (Cell Cell in Row.Cells)
				{
					ColumnNr++;
					if (ColumnNr > NrColumns)
					{
						NrColumns++;
						ColumnAlignmentCounts.Add(new int[3]);
					}

					int[] Alignments = ColumnAlignmentCounts[ColumnNr - 1];

					switch (Cell.Alignment)
					{
						case CellAlignment.Left:
							Alignments[0]++;
							break;

						case CellAlignment.Right:
							Alignments[1]++;
							break;

						case CellAlignment.Center:
							Alignments[2]++;
							break;
					}
				}
			}

			int i;
			ColumnAlignments = new TextAlignment[NrColumns];

			for (i = 0; i < NrColumns; i++)
			{
				int[] Counts = ColumnAlignmentCounts[i];

				if (Counts[0] >= Counts[1])
				{
					if (Counts[0] >= Counts[2])
						ColumnAlignments[i] = TextAlignment.Left;
					else
						ColumnAlignments[i] = TextAlignment.Center;
				}
				else
				{
					if (Counts[1] >= Counts[2])
						ColumnAlignments[i] = TextAlignment.Right;
					else
						ColumnAlignments[i] = TextAlignment.Center;
				}
			}

			bool InHeader = true;

			for (i = 0; i < NrRows; i++)
			{
				Row Row = this.Rows[i];

				if (InHeader && !IsRowHeader[i])
				{
					if (i == 0)	// No header in table.
					{
						Markdown.Indent(Indentation);
						Markdown.Append("| ");
						Markdown.Append(new string('|', NrColumns));
						Markdown.AppendLine();
					}

					InHeader = false;
					Markdown.Indent(Indentation);
					Markdown.Append('|');

					foreach (TextAlignment Alignment in ColumnAlignments)
					{
						switch (Alignment)
						{
							case TextAlignment.Left:
								Markdown.Append(":---|");
								break;

							case TextAlignment.Right:
								Markdown.Append("---:|");
								break;

							case TextAlignment.Center:
								Markdown.Append(":--:|");
								break;

							default:
								Markdown.Append("----|");
								break;
						}
					}

					Markdown.AppendLine();
				}

				Markdown.Indent(Indentation);

				foreach (Cell Cell in Row.Cells)
				{
					Markdown.Append("| ");

					MarkdownOutput CellMarkdown = new MarkdownOutput();
					await Cell.GenerateMarkdown(CellMarkdown, 1, 0, Settings);

					string[] Rows = CellMarkdown.ToString().Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

					if (Rows.Length == 1)
						Markdown.Append(Rows[0]);
					else
					{
						if (Notes is null)
							Notes = new LinkedList<KeyValuePair<string, string[]>>();

						string Key = "n" + Guid.NewGuid().ToString().Replace("-", string.Empty);

						Notes.AddLast(new KeyValuePair<string, string[]>(Key, Rows));

						Markdown.Append("[^");
						Markdown.Append(Key);
						Markdown.Append(']');
					}

					if (Cell.ColumnSpan > 1)
						Markdown.Append(new string('|', Cell.ColumnSpan - 1));
				}

				Markdown.Append("|");
				Markdown.AppendLine();
			}

			Markdown.Indent(Indentation);
			Markdown.AppendLine();

			if (!(Notes is null))
			{
				foreach (KeyValuePair<string, string[]> Note in Notes)
				{
					bool First = true;

					Markdown.Indent(Indentation);
					Markdown.Append("[^");
					Markdown.Append(Note.Key);
					Markdown.Append("]:\t");

					foreach (string Row in Note.Value)
					{
						if (First)
							First = false;
						else
							Markdown.Indent(Indentation + 1);

						Markdown.Append(Row);
						Markdown.AppendLine();
					}

					Markdown.Indent(Indentation);
					Markdown.AppendLine();
				}
			}
		}
	}
}
