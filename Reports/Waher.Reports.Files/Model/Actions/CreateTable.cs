using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Things.Queries;

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
		private readonly int nrColumns;

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
			this.nrColumns = this.columns.Length;
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			string TableId = await this.tableId.Evaluate(State.Variables);
			string Name = await this.name.Evaluate(State.Variables);

			Column[] Columns = new Column[this.nrColumns];
			int i;

			for (i = 0; i < this.nrColumns; i++)
				Columns[i] = await this.columns[i].Evaluate(State);

			await State.Query.NewTable(TableId, Name, Columns);

			return true;
		}
	}
}
