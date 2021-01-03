using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Consolidates Markdown from multiple sources, sharing the same thread.
	/// </summary>
	public class ThreadConsolidation
	{
		private readonly Regex tableHeadline = new Regex(@"^[|]?(:?-+:?[|]*)+$", RegexOptions.Singleline | RegexOptions.Compiled);
		private readonly SortedDictionary<string, SourceState> sources = new SortedDictionary<string, SourceState>();
		private readonly string threadId;
		private DocumentType type = DocumentType.Empty;
		private object tag = null;
		private int nrTop = 0;
		private int nrBottom = 0;
		private int nrHeader = 0;

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
				this.nrHeader = 0;

				if (this.type == DocumentType.SingleCode || this.type == DocumentType.SingleTable)
				{
					int i, c, d, d0 = 0;
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
							d0 = d;
							this.nrHeader = 0;

							for (i = 0; i < d; i++)
							{
								if (this.tableHeadline.IsMatch(Rows[i]))
									this.nrHeader = i + 1;
								else if (this.nrHeader > 0)
									break;
							}
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
								if (Rows[d - i - 1] != Rows0[d0 - i - 1])
									break;
							}

							this.nrBottom = i;
						}
					}

					switch (this.type)
					{
						case DocumentType.SingleCode:
							if (this.nrTop < 1 || this.nrBottom <= 1)
								this.type = DocumentType.Complex;
							break;

						case DocumentType.SingleTable:
							if (this.nrTop < this.nrHeader)
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

					case DocumentType.SingleTable:

						j = 0;
						d = this.sources.Count;

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
							{
								bool Headline = false;

								if (i < this.nrHeader)
								{
									if (tableHeadline.IsMatch(Rows[i]))
									{
										Markdown.Append("|---");
										Headline = true;
									}
									else if (i == 0)
										Markdown.Append("| Source ");
									else
										Markdown.Append("| ");
								}
								else if (i >= Rows.Length - this.nrBottom)
									Markdown.Append("| ");
								else
								{
									Markdown.Append("| `");
									Markdown.Append(P.Key);
									Markdown.Append("` ");
								}

								if (!Rows[i].StartsWith("|"))
								{
									Markdown.Append('|');
									if (!Headline)
										Markdown.Append(' ');
								}

								Markdown.AppendLine(Rows[i]);
							}
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
