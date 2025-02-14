using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a column in a table.
	/// </summary>
	public class TableColumn
	{
		private readonly ReportStringAttribute columnId;
		private readonly ReportStringAttribute header;
		private readonly ReportStringAttribute dataSourceId;
		private readonly ReportStringAttribute partition;
		private readonly ReportStringAttribute fgColor;
		private readonly ReportStringAttribute bgColor;
		private readonly ReportEnumAttribute<ColumnAlignment> alignment;
		private readonly ReportByteAttribute nrDecimals;

		/// <summary>
		/// Defines a column in a table.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public TableColumn(XmlElement Xml)
		{
			this.columnId = new ReportStringAttribute(Xml, "columnId");
			this.header = new ReportStringAttribute(Xml, "header");
			this.dataSourceId = new ReportStringAttribute(Xml, "dataSourceId");
			this.partition = new ReportStringAttribute(Xml, "partition");
			this.fgColor = new ReportStringAttribute(Xml, "fgColor");
			this.bgColor = new ReportStringAttribute(Xml, "bgColor");
			this.alignment = new ReportEnumAttribute<ColumnAlignment>(Xml, "alignment");
			this.nrDecimals = new ReportByteAttribute(Xml, "nrDecimals");
		}

	}
}
