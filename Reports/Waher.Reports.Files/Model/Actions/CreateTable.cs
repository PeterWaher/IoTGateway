using System.Collections.Generic;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Creates a table in the report
	/// </summary>
	public class CreateTable : ReportAction
	{
		private readonly ReportStringAttribute tableId;
		private readonly ReportStringAttribute name;
		private readonly TableColumn[] columns;

		/// <summary>
		/// Creates a table in the report
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public CreateTable(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.tableId = new ReportStringAttribute(Xml, "tableId");
			this.name = new ReportStringAttribute(Xml, "name");

			List<TableColumn> TableColumns = new List<TableColumn>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "TableColumn")
					TableColumns.Add(new TableColumn(E));
			}

			this.columns = TableColumns.ToArray();
		}
	}
}
