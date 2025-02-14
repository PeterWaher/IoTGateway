using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports records to a table.
	/// </summary>
	public class TableRecords : ReportAction
	{
		private readonly ReportStringAttribute tableId;
		private readonly TableRecord[] records;
		private readonly int nrRecords;

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
			this.nrRecords = this.records.Length;
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			string TableId = await this.tableId.Evaluate(State.Variables);
			Record[] Records = new Record[this.nrRecords];
			int i;

			for (i=0;i<this.nrRecords;i++)
				Records[i] = new Record(await this.records[i].Evaluate(State));

			await State.Query.NewRecords(TableId, Records);

			return true;
		}
	}
}
