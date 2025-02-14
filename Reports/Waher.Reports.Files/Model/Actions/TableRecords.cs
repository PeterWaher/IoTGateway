using System.Collections.Generic;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports records to a table.
	/// </summary>
	public class TableRecords : ReportAction
	{
		private readonly ReportStringAttribute tableId;
		private readonly TableRecord[] records;

		/// <summary>
		/// Reports records to a table.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public TableRecords(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.tableId = new ReportStringAttribute(Xml, "tableId");

			List<TableRecord> TableRecords = new List<TableRecord>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Record")
					TableRecords.Add(new TableRecord(E));
			}

			this.records = TableRecords.ToArray();
		}
	}
}
