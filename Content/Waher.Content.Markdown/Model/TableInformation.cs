using System;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Text alignment of contents.
	/// </summary>
	public enum TextAlignment
	{
		/// <summary>
		/// Left alignment.
		/// </summary>
		Left,

		/// <summary>
		/// Center alignment.
		/// </summary>
		Center,

		/// <summary>
		/// Right alignment.
		/// </summary>
		Right
	}

	internal class TableInformation
	{
		public int Columns;
		public int NrHeaderRows;
		public int NrDataRows;
		public string[][] Headers;
		public string[][] Rows;
		public int[][] HeaderPositions;
		public int[][] RowPositions;
		public TextAlignment[] Alignments;
		public string[] AlignmentDefinitions;
		public string Caption;
		public string Id;
	}
}
