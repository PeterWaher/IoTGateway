using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Consolidates Markdown from multiple sources, sharing the same thread.
	/// </summary>
	public class ThreadConsolidation
	{
		private readonly SortedDictionary<string, SourceState> sources = new SortedDictionary<string, SourceState>();
		private readonly string threadId;
		private DocumentType type = DocumentType.Empty;
		private object tag = null;
		private int nrTop = 0;
		private int nrBottom = 0;

		/// <summary>
		/// Consolidates Markdown from multiple sources, sharing the same thread.
		/// </summary>
		/// <param name="ThreadId">Thread ID</param>
		public ThreadConsolidation(string ThreadId)
		{
			this.threadId = ThreadId;
		}

		/// <summary>
		/// Thread ID
		/// </summary>
		public string ThreadId => this.threadId;

		/// <summary>
		/// Consolidated sources.
		/// </summary>
		public string[] Sources
		{
			get
			{
				lock (this.sources)
				{
					string[] Result = new string[this.sources.Count];
					this.sources.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// External tag object that can be tagged to the object by its owner.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// Adds incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Markdown">Markdown document.</param>
		/// <returns>If the source is new.</returns>
		public bool Add(string Source, MarkdownDocument Markdown)
		{
			bool Result;

			lock (this.sources)
			{
				if (Result = !this.sources.TryGetValue(Source, out SourceState State))
				{
					State = new SourceState(Source);
					this.sources[Source] = State;
				}

				DocumentType Type = State.Add(Markdown);

				if ((int)(this.type & Type) != 0)
					this.type = (DocumentType)Math.Max((int)this.type, (int)Type);
				else
					this.type = DocumentType.Complex;

				this.nrTop = 0;
				this.nrBottom = 0;

				if (this.type == DocumentType.SingleCode || this.type == DocumentType.SingleTable)
				{
					int i, j, c, d;
					bool First = true;
					string[] Rows0 = null;
					string[] Rows;

					foreach (SourceState Info in this.sources.Values)
					{
						Rows = Info.FirstDocument.Rows;
						d = Rows.Length;

						if (First)
						{
							First = false;
							this.nrTop = this.nrBottom = d;
							Rows0 = Rows;
						}
						else
						{
							c = Math.Min(this.nrTop, d);

							for (i = 0; i < c; i++)
							{
								if (Rows[i] != Rows0[i])
									break;
							}

							this.nrTop = i;

							c = Math.Min(this.nrBottom, d);

							for (i = 0; i < c; i++)
							{
								j = d - i - 1;
								if (Rows[j] != Rows0[j])
									break;
							}

							this.nrBottom = i;
						}
					}

					switch (this.type)
					{
						case DocumentType.SingleCode:
							if (this.nrTop <= 1 || this.nrBottom <= 1)
								this.type = DocumentType.Complex;
							break;

						case DocumentType.SingleTable:
							if (this.nrTop <= 2)
								this.type = DocumentType.Complex;
							break;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Generates consolidated markdown from all sources.
		/// </summary>
		/// <returns>Consolidated markdown.</returns>
		public string GenerateMarkdown()
		{
			StringBuilder Markdown = new StringBuilder();

			lock (this.sources)
			{
				switch (this.type)
				{
					case DocumentType.Empty:
						return string.Empty;

					case DocumentType.SingleNumber:
					case DocumentType.SingleLine:

						Markdown.AppendLine("| Source | Response |");

						if (this.type == DocumentType.SingleNumber)
							Markdown.AppendLine("|:-------|-------:|");
						else
							Markdown.AppendLine("|:-------|:-------|");

						foreach (KeyValuePair<string, SourceState> P in this.sources)
						{
							Markdown.Append("| `");
							Markdown.Append(P.Key);
							Markdown.Append("` | ");
							Markdown.Append(P.Value.FirstText);
							Markdown.AppendLine(" |");
						}
						break;

					case DocumentType.SingleParagraph:

						Markdown.AppendLine("| Source | Response |");
						Markdown.AppendLine("|:-------|:-------|");

						foreach (KeyValuePair<string, SourceState> P in this.sources)
						{
							Markdown.Append("| `");
							Markdown.Append(P.Key);
							Markdown.Append("` | ");

							foreach (string Row in P.Value.FirstDocument.Rows)
							{
								Markdown.Append(Row);
								Markdown.Append(' ');
							}

							Markdown.AppendLine("|");
						}
						break;

					case DocumentType.SingleCode:
					case DocumentType.SingleTable:

						int j = 0;
						int d = this.sources.Count;

						foreach (KeyValuePair<string, SourceState> P in this.sources)
						{
							string[] Rows = P.Value.FirstDocument.Rows;
							int i = 0;
							int c = Rows.Length;

							j++;
							if (j > 1)
								i += this.nrTop;

							if (j < d)
								c -= this.nrBottom;

							for (; i < c; i++)
								Markdown.AppendLine(Rows[i]);
						}
						break;

					case DocumentType.Complex:
					default:
						foreach (KeyValuePair<string, SourceState> P in this.sources)
						{
							Markdown.Append('`');
							Markdown.Append(P.Key);
							Markdown.AppendLine("`");
							Markdown.AppendLine();

							bool First = true;

							foreach (DocumentInformation Doc in P.Value.Documents)
							{
								if (First)
									First = false;
								else
									Markdown.AppendLine(":\t");

								foreach (string Row in Doc.Rows)
								{
									Markdown.Append(":\t");
									Markdown.AppendLine(Row);
								}
							}

							Markdown.AppendLine();
						}
						break;
				}
			}

			return Markdown.ToString();
		}

	}
}
