using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a table in a markdown document.
	/// </summary>
	public class Table : MarkdownElement
	{
		private MarkdownElement[][] headers;
		private MarkdownElement[][] rows;
		private TableCellAlignment[] alignments;
		private string caption;
		private string id;
		private int columns;

		/// <summary>
		/// Represents a table in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Columns">Columns in table.</param>
		/// <param name="Headers">Header rows.</param>
		/// <param name="Rows">Data rows.</param>
		/// <param name="Alignments">Column alignments.</param>
		public Table(MarkdownDocument Document, int Columns, MarkdownElement[][] Headers, MarkdownElement[][] Rows, TableCellAlignment[] Alignments,
			string Caption, string Id)
			: base(Document)
		{
			this.columns = Columns;
			this.headers = Headers;
			this.rows = Rows;
			this.alignments = Alignments;
			this.caption = Caption;
			this.id = Id;
		}

		/// <summary>
		/// Headers in table.
		/// </summary>
		public MarkdownElement[][] Headers
		{
			get { return this.headers; }
		}

		/// <summary>
		/// Rows in table.
		/// </summary>
		public MarkdownElement[][] Rows
		{
			get { return this.rows; }
		}

		/// <summary>
		/// Table cell alignments.
		/// </summary>
		public TableCellAlignment[] Alignments
		{
			get { return this.alignments; }
		}

		/// <summary>
		/// Table caption.
		/// </summary>
		public string Caption
		{
			get { return this.caption; }
		}

		/// <summary>
		/// ID of table.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			MarkdownElement E;
			int i, j, k;

			Output.AppendLine("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\">");

			if (!string.IsNullOrEmpty(this.id))
			{
				Output.Append("<caption id=\"");
				Output.Append(MarkdownDocument.HtmlAttributeEncode(this.id));
				Output.Append("\">");

				if (string.IsNullOrEmpty(this.caption))
					Output.Append(MarkdownDocument.HtmlValueEncode(this.id));
				else
					Output.Append(MarkdownDocument.HtmlValueEncode(this.caption));

				Output.AppendLine("</caption>");
			}

			Output.AppendLine("<colgroup>");
			foreach (TableCellAlignment Alignment in this.alignments)
			{
				Output.Append("<col style=\"text-align:");
				Output.Append(Alignment.ToString().ToLower());
				Output.AppendLine("\"/>");
			}
			Output.AppendLine("</colgroup>");

			Output.AppendLine("<thead>");
			foreach (MarkdownElement[] Row in this.headers)
			{
				Output.AppendLine("<tr>");

				for (i = 0; i < this.columns; i++)
				{
					E = Row[i];
					if (E == null)
						continue;

					k = 1;
					j = i + 1;
					while (j < this.columns && Row[j++] == null)
						k++;

					Output.Append("<th style=\"text-align:");
					Output.Append(this.alignments[i].ToString().ToLower());

					if (k > 1)
					{
						Output.Append("\" colspan=\"");
						Output.Append(k.ToString());
					}

					Output.Append("\">");
					E.GenerateHTML(Output);
					Output.AppendLine("</th>");
				}

				Output.AppendLine("</tr>");
			}
			Output.AppendLine("</thead>");

			Output.AppendLine("<tbody>");
			foreach (MarkdownElement[] Row in this.rows)
			{
				Output.AppendLine("<tr>");

				for (i = 0; i < this.columns; i++)
				{
					E = Row[i];
					if (E == null)
						continue;

					k = 1;
					j = i + 1;
					while (j < this.columns && Row[j++] == null)
						k++;

					Output.Append("<td style=\"text-align:");
					Output.Append(this.alignments[i].ToString().ToLower());

					if (k > 1)
					{
						Output.Append("\" colspan=\"");
						Output.Append(k.ToString());
					}

					Output.Append("\">");
					E.GenerateHTML(Output);
					Output.AppendLine("</td>");
				}

				Output.AppendLine("</tr>");
			}
			Output.AppendLine("</tbody>");

			Output.AppendLine("</table>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			bool First;

			foreach (MarkdownElement[] Row in this.headers)
			{
				First = true;

				foreach (MarkdownElement E in Row)
				{
					if (E == null)
						continue;

					if (First)
						First = false;
					else
						Output.Append('\t');

					E.GeneratePlainText(Output);
				}

				Output.AppendLine();
			}

			foreach (MarkdownElement[] Row in this.rows)
			{
				First = true;

				foreach (MarkdownElement E in Row)
				{
					if (E == null)
						continue;

					if (First)
						First = false;
					else
						Output.Append('\t');

					E.GeneratePlainText(Output);
				}

				Output.AppendLine();
			}

			Output.AppendLine();
		}
	}
}
