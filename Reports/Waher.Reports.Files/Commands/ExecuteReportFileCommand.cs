using System.Threading.Tasks;
using Waher.Reports.Files.Model;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Commands
{
	/// <summary>
	/// Command to execute a report file.
	/// </summary>
	public class ExecuteReportFileCommand : ExecuteReport
	{
		private readonly ReportFile parsedReport;

		/// <summary>
		/// Command to execute a report file.
		/// </summary>
		/// <param name="Report">Parsed report file.</param>
		/// <param name="ReportNode">Report node.</param>
		public ExecuteReportFileCommand(ReportFile Report, ReportNode ReportNode)
			: base(ReportNode)
		{
			this.parsedReport = Report;
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public override ICommand Copy()
		{
			return new ExecuteReportFileCommand(this.parsedReport, this.Report);
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public override Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;	// TODO
		}
	}
}
