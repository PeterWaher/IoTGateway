using SkiaSharp;
using System.Threading.Tasks;
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
		private readonly ReportColorAttribute fgColor;
		private readonly ReportColorAttribute bgColor;
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
			this.fgColor = new ReportColorAttribute(Xml, "fgColor");
			this.bgColor = new ReportColorAttribute(Xml, "bgColor");
			this.alignment = new ReportEnumAttribute<ColumnAlignment>(Xml, "alignment");
			this.nrDecimals = new ReportByteAttribute(Xml, "nrDecimals");
		}

		/// <summary>
		/// Evaluates the record definition.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>Record</returns>
		public async Task<Column> Evaluate(ReportState State)
		{
			string ColumnId = await this.columnId.Evaluate(State.Variables);
			string Header = await this.header.Evaluate(State.Variables);
			string DataSourceId = await this.dataSourceId.Evaluate(State.Variables);
			string Partition = await this.partition.Evaluate(State.Variables);
			SKColor FgColor = await this.fgColor.Evaluate(State.Variables);
			SKColor BgColor = await this.bgColor.Evaluate(State.Variables);
			ColumnAlignment Alignment = await this.alignment.Evaluate(State.Variables);
			byte NrDecimals = await this.nrDecimals.Evaluate(State.Variables);

			return new Column(ColumnId, Header, DataSourceId, Partition, FgColor, BgColor, Alignment, NrDecimals);
		}

	}
}
