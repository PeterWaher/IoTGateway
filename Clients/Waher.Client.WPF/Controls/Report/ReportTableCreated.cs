using System;
using System.Xml;
using Waher.Things.Queries;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Creation of a table.
	/// </summary>
	public class ReportTableCreated : ReportElement
	{
		private readonly string tableId;
		private readonly string name;
		private readonly Column[] columns;

		/// <summary>
		/// Creation of a table.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Name">Table name</param>
		/// <param name="Columns">Columns</param>
		public ReportTableCreated(string TableId, string Name, Column[] Columns)
		{
			this.tableId = TableId;
			this.name = Name;
			this.columns = Columns;
		}

		/// <summary>
		/// Table ID
		/// </summary>
		public string TableId => this.tableId;

		/// <summary>
		/// Table name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Columns
		/// </summary>
		public Column[] Columns => this.columns;

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("TableStart");
			Output.WriteAttributeString("tableId", this.tableId);
			Output.WriteAttributeString("name", this.name);

			if (!(this.columns is null))
			{
				foreach (Column Column in this.columns)
				{
					Output.WriteStartElement("Column");
					Output.WriteAttributeString("columnId", Column.ColumnId);
					Output.WriteAttributeString("header", Column.Header);

					if (!string.IsNullOrEmpty(Column.DataSourceId))
						Output.WriteAttributeString("dataSourceId", Column.DataSourceId);

					if (!string.IsNullOrEmpty(Column.Partition))
						Output.WriteAttributeString("partition", Column.Partition);

					if (Column.FgColor.HasValue)
						Output.WriteAttributeString("fgColor", Column.FgColor.ToString());

					if (Column.BgColor.HasValue)
						Output.WriteAttributeString("bgColor", Column.BgColor.ToString());

					if (Column.Alignment.HasValue)
						Output.WriteAttributeString("alignment", Column.Alignment.ToString());

					if (Column.NrDecimals.HasValue)
						Output.WriteAttributeString("nrDecimals", Column.NrDecimals.ToString());
					
					Output.WriteEndElement();
				}
			}

			Output.WriteEndElement();
		}
	}
}
