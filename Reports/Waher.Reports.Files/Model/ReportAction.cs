using System.Threading.Tasks;

namespace Waher.Reports.Files.Model
{
	/// <summary>
	/// Abstract base class for report actions.
	/// </summary>
	public abstract class ReportAction
	{
		private readonly ReportFile report;

		/// <summary>
		/// Abstract base class for report actions.
		/// </summary>
		/// <param name="Report">Parsed report.</param>
		public ReportAction(ReportFile Report)
		{
			this.report = Report;
		}

		/// <summary>
		/// Parsed report containing the report action.
		/// </summary>
		public ReportFile Report => this.report;

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public abstract Task<bool> Execute(ReportState State);
	}
}
