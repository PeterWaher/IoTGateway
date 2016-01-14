using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a table in a markdown document.
	/// </summary>
	public class Table : MarkdownElement
	{
		private MarkdownElement[][] headers;
		private MarkdownElement[][] rows;
		private TextAlignment[] alignments;
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
		public Table(MarkdownDocument Document, int Columns, MarkdownElement[][] Headers, MarkdownElement[][] Rows, TextAlignment[] Alignments,
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
		public TextAlignment[] Alignments
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
			foreach (TextAlignment Alignment in this.alignments)
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

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			int Column;
			int Row, NrRows;
			int RowNr = 0;

			Output.WriteStartElement("Grid");
			Output.WriteAttributeString("ShowGridLines", "True");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			if (!string.IsNullOrEmpty(this.caption))
				Output.WriteAttributeString("ToolTip", this.caption);

			Output.WriteStartElement("Grid.ColumnDefinitions");

			for (Column = 0; Column < this.columns; Column++)
			{
				Output.WriteStartElement("ColumnDefinition");
				Output.WriteAttributeString("Width", "Auto");
				Output.WriteEndElement();
			}

			Output.WriteEndElement();
			Output.WriteStartElement("Grid.RowDefinitions");

			for (Row = 0, NrRows = this.rows.Length + this.headers.Length; Row < NrRows; Row++)
			{
				Output.WriteStartElement("RowDefinition");
				Output.WriteAttributeString("Height", "Auto");
				Output.WriteEndElement();
			}

			Output.WriteEndElement();

			for (Row = 0, NrRows = this.headers.Length; Row < NrRows; Row++, RowNr++)
				this.GenerateXAML(Output, Settings, this.headers[Row], RowNr, true);

			for (Row = 0, NrRows = this.rows.Length; Row < NrRows; Row++, RowNr++)
				this.GenerateXAML(Output, Settings, this.rows[Row], RowNr, false);

			Output.WriteEndElement();
		}

		private void GenerateXAML(XmlWriter Output, XamlSettings Settings, MarkdownElement[] CurrentRow, int RowNr, bool Bold)
		{
			MarkdownElement E;
			TextAlignment TextAlignment;
			int Column;
			int NrColumns;
			int ColSpan;

			for (Column = 0, NrColumns = CurrentRow.Length; Column < NrColumns; Column++)
			{
				E = CurrentRow[Column];
				if (E == null)
					continue;

				TextAlignment = this.alignments[Column];
				ColSpan = Column + 1;
				while (ColSpan < NrColumns && CurrentRow[ColSpan] == null)
					ColSpan++;

				ColSpan -= Column;

				if (E.InlineSpanElement && !E.OutsideParagraph)
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
					Output.WriteAttributeString("Padding", Settings.TableCellPadding);

					if (Bold)
						Output.WriteAttributeString("FontWeight", "Bold");

					if (TextAlignment != TextAlignment.Left)
						Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());
				}
				else
				{
					Output.WriteStartElement("StackPanel");
					Output.WriteAttributeString("Margin", Settings.TableCellPadding);
				}

				Output.WriteAttributeString("Grid.Column", Column.ToString());
				Output.WriteAttributeString("Grid.Row", RowNr.ToString());

				if (ColSpan > 1)
					Output.WriteAttributeString("Grid.ColumnSpan", ColSpan.ToString());

				E.GenerateXAML(Output, Settings, TextAlignment);
				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}
	}
}
