using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports a table to be complete.
	/// </summary>
	public class TableComplete : ReportAction
	{
		private readonly ReportStringAttribute tableId;

		/// <summary>
		/// Reports a table to be complete.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public TableComplete(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.tableId = new ReportStringAttribute(Xml, "tableId");
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			string TableId = await this.tableId.Evaluate(State.Variables);

			await State.Query.TableDone(TableId);

			return true;
		}
	}
}
