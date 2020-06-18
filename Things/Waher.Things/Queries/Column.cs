using System;
using SkiaSharp;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Column alignment.
	/// </summary>
	public enum ColumnAlignment
	{
		/// <summary>
		/// Left aligned.
		/// </summary>
		Left,

		/// <summary>
		/// Center aligned.
		/// </summary>
		Center,

		/// <summary>
		/// Right aligned.
		/// </summary>
		Right
	}

	/// <summary>
	/// Defines a column in a table.
	/// </summary>
	public class Column
	{
		private readonly string columnId;
		private readonly string header;
		private readonly string dataSourceId;
		private readonly string partition;
		private readonly SKColor? fgColor;
		private readonly SKColor? bgColor;
		private readonly ColumnAlignment? alignment;
		private readonly byte? nrDecimals;

		/// <summary>
		/// Defines a column in a table.
		/// </summary>
		/// <param name="ColumnId">Column ID</param>
		/// <param name="Header">Optional localized header.</param>
		/// <param name="DataSourceId">Optional Data Suorce ID reference.</param>
		/// <param name="Partition">Optional partition reference.</param>
		/// <param name="FgColor">Optional Foreground Color.</param>
		/// <param name="BgColor">Optional Background Color.</param>
		/// <param name="Alignment">Optional Column Alignment.</param>
		/// <param name="NrDecimals">Optional Number of Decimals.</param>
		public Column(string ColumnId, string Header, string DataSourceId, string Partition, SKColor? FgColor, SKColor? BgColor,
			ColumnAlignment? Alignment, byte? NrDecimals)
		{
			this.columnId = ColumnId;
			this.header = Header;
			this.dataSourceId = DataSourceId;
			this.partition = Partition;
			this.fgColor = FgColor;
			this.bgColor = BgColor;
			this.alignment = Alignment;
			this.nrDecimals = NrDecimals;
		}

		/// <summary>
		/// Column ID
		/// </summary>
		public string ColumnId
		{
			get { return this.columnId; }
		}

		/// <summary>
		/// Optional localized header.
		/// </summary>
		public string Header
		{
			get { return this.header; }
		}

		/// <summary>
		/// Optional Data Suorce ID reference.
		/// </summary>
		public string DataSourceId
		{
			get { return this.dataSourceId; }
		}

		/// <summary>
		/// Optional partition reference.
		/// </summary>
		public string Partition
		{
			get { return this.partition; }
		}

		/// <summary>
		/// Optional Foreground Color.
		/// </summary>
		public SKColor? FgColor
		{
			get { return this.fgColor; }
		}

		/// <summary>
		/// Optional Background Color.
		/// </summary>
		public SKColor? BgColor
		{
			get { return this.bgColor; }
		}

		/// <summary>
		/// Optional Column Alignment.
		/// </summary>
		public ColumnAlignment? Alignment
		{
			get { return this.alignment; }
		}

		/// <summary>
		/// Optional Number of Decimals.
		/// </summary>
		public byte? NrDecimals
		{
			get { return this.nrDecimals; }
		}
	}
}
