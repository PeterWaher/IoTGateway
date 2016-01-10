using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	public enum TableCellAlignment
	{
		Left,
		Center,
		Right
	}

	internal class TableInformation
	{
		public int Columns;
		public int NrHeaderRows;
		public int NrDataRows;
		public string[][] Headers;
		public string[][] Rows;
		public TableCellAlignment[] Alignments;
		public string Caption;
		public string Id;
	}
}
