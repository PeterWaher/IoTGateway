﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Xml;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Type of markdown document.
	/// </summary>
	[Flags]
	public enum DocumentType
	{
		/// <summary>
		/// Empty document
		/// </summary>
		Empty = 1,

		/// <summary>
		/// Contains a single line containing a number.
		/// </summary>
		SingleNumber = 3,

		/// <summary>
		/// Contains a single line of text.
		/// </summary>
		SingleLine = 7,

		/// <summary>
		/// Contains a single paragraph of text.
		/// </summary>
		SingleParagraph = 15,

		/// <summary>
		/// Contains one code section.
		/// </summary>
		SingleCode = 17,

		/// <summary>
		/// Contains one table.
		/// </summary>
		SingleTable = 33,

		/// <summary>
		/// Contains one graph.
		/// </summary>
		SingleGraph = 65,

		/// <summary>
		/// Contains one block of XML
		/// </summary>
		SingleXml = 129,

		/// <summary>
		/// Contains complex content.
		/// </summary>
		Complex = 511
	}

	/// <summary>
	/// Information about a document.
	/// </summary>
	public class DocumentInformation
	{
		private MarkdownDocument markdown;
		private DocumentType type;
		private string[] rows;
		private Type graphType;
		private Graph graph;
		private Table table;
		private string id;

		private DocumentInformation()
		{
		}

		/// <summary>
		/// Information about a document.
		/// </summary>
		/// <param name="Markdown">Markdown document</param>
		/// <param name="Id">Optional ID of document</param>
		public static async Task<DocumentInformation> CreateAsync(MarkdownDocument Markdown, string Id)
		{
			DocumentInformation Result = new DocumentInformation();
			MarkdownElement First = null;
			bool IsTable = false;
			bool IsCode = false;
			ChunkNode<MarkdownElement> Loop = Markdown.Elements.FirstChunk;
			MarkdownElement E;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					E = Loop[i];
					First ??= E;

					i++;
					IsTable |= E is Table;
					IsCode |= E is CodeBlock;
				}

				Loop = Loop.Next;
			}

			i = Markdown.Elements.Count;

			Result.markdown = Markdown;
			Result.id = Id;

			string s = await Markdown.GenerateMarkdown(false);
			s = s.Trim().Replace("\r\n", "\n").Replace('\r', '\n');
			Result.rows = s.Split('\n');

			if (i == 0)
				Result.type = DocumentType.Empty;
			else if (i == 1)
			{
				c = Result.rows.Length;

				if (IsTable)
				{
					Result.table = (Table)First;
					Result.type = DocumentType.SingleTable;
				}
				else if (IsCode)
				{
					if (c >= 3 &&
						Result.rows[0].StartsWith("```", StringComparison.CurrentCultureIgnoreCase) &&
						Result.rows[1].StartsWith("<", StringComparison.CurrentCultureIgnoreCase) &&
						Result.rows[c - 2].EndsWith(">", StringComparison.CurrentCultureIgnoreCase) &&
						Result.rows[c - 1].EndsWith("```"))
					{
						if (c >= 4 &&
							Result.rows[0].StartsWith("```Graph", StringComparison.CurrentCultureIgnoreCase) &&
							Result.rows[1].StartsWith("<Graph", StringComparison.CurrentCultureIgnoreCase) &&
							Result.rows[c - 2].EndsWith("</Graph>", StringComparison.CurrentCultureIgnoreCase) &&
							Result.rows[c - 1].EndsWith("```"))
						{
							try
							{
								StringBuilder sb = new StringBuilder();

								for (i = 1, c--; i < c; i++)
									sb.AppendLine(Result.rows[i]);

								XmlDocument Xml = new XmlDocument();
								Xml.LoadXml(sb.ToString());

								Graph G = await Graph.TryImport(Xml.DocumentElement);

								if (G is null)
									Result.type = DocumentType.SingleXml;
								else
								{
									Result.graph = G;
									Result.graphType = G.GetType();
									Result.type = DocumentType.SingleGraph;
								}
							}
							catch (Exception)
							{
								Result.type = DocumentType.SingleXml;
							}
						}
						else
							Result.type = DocumentType.SingleXml;
					}
					else
						Result.type = DocumentType.SingleCode;
				}
				else
				{
					if (string.IsNullOrEmpty(s))
						Result.type = DocumentType.Empty;
					else
					{
						if (c > 1)
							Result.type = DocumentType.SingleParagraph;
						else if (IsNumeric(Result.rows[0]))
							Result.type = DocumentType.SingleNumber;
						else
							Result.type = DocumentType.SingleLine;
					}
				}
			}
			else
				Result.type = DocumentType.Complex;

			return Result;
		}

		private static bool IsNumeric(string s)
		{
			if (s.StartsWith("<") && s.EndsWith(">"))
			{
				int i = s.IndexOf('>');
				int j = s.LastIndexOf('<');

				return (i < j && IsNumeric(s.Substring(i + 1, j - i - 1)));
			}
			else if (s.Length > 2 && s.StartsWith("`") && s.EndsWith("`"))
				return IsNumeric(s[1..^1]);
			else
				return CommonTypes.TryParse(s, out double _);
		}

		/// <summary>
		/// ID of record.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Markdown document.
		/// </summary>
		public MarkdownDocument Markdown => this.markdown;

		/// <summary>
		/// Document type.
		/// </summary>
		public DocumentType Type => this.type;

		/// <summary>
		/// Graph object, if <see cref="DocumentType.SingleGraph"/>
		/// </summary>
		public Graph Graph => this.graph;

		/// <summary>
		/// Table object, if <see cref="DocumentType.SingleTable"/>
		/// </summary>
		public Table Table => this.table;

		/// <summary>
		/// Rows
		/// </summary>
		public string[] Rows => this.rows;
	}
}
