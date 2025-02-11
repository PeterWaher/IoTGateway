using System;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Reports
{
	/// <summary>
	/// Abstract base class for commands executing a report.
	/// </summary>
	public abstract class ExecuteReport : ICommand
	{
		private readonly ReportNode report;

		/// <summary>
		/// Abstract base class for commands executing a report.
		/// </summary>
		/// <param name="Report">Report node.</param>
		public ExecuteReport(ReportNode Report)
		{
			this.report = Report;
		}

		/// <summary>
		/// Report node.
		/// </summary>
		public ReportNode Report => this.report;

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "Execute";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Report";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => this.CommandID;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public virtual Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ReportsDataSource), 4, "Execute Report...");
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language) => Task.FromResult(string.Empty);

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language) => Task.FromResult(string.Empty);

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language) => Task.FromResult(string.Empty);

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public async Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return await this.report.CanViewAsync(Caller);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public abstract Task StartQueryExecutionAsync(Query Query, Language Language);

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public abstract ICommand Copy();
	}
}
