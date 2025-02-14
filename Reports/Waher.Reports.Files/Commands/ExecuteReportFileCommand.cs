using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Reports.Files.Model;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Commands
{
	/// <summary>
	/// Command to execute a report file.
	/// </summary>
	public class ExecuteReportFileCommand : ExecuteReport, IEditableObject
	{
		private readonly ReportFile parsedReport;
		private readonly Variables variables;

		/// <summary>
		/// Command to execute a report file.
		/// </summary>
		/// <param name="Report">Parsed report file.</param>
		/// <param name="ReportNode">Report node.</param>
		/// <param name="Variables">Variable value collection.</param>
		public ExecuteReportFileCommand(ReportFile Report, ReportNode ReportNode, Variables Variables)
			: base(ReportNode)
		{
			this.parsedReport = Report;
			this.variables = Variables;
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public override ICommand Copy()
		{
			return new ExecuteReportFileCommand(this.parsedReport, this.Report, this.variables);
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		public Task PopulateForm(DataForm Parameters, Language Language)
		{
			return this.parsedReport.PopulateForm(Parameters, Language, this.variables);
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language, bool OnlySetChanged)
		{
			return this.parsedReport.SetParameters(Parameters, Language, OnlySetChanged, this.variables);
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public override async Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			ReportState State = new ReportState(Query, Language, this.variables,
				await Language.GetNamespaceAsync(typeof(ReportFileNode).Namespace));

			try
			{
				if (!State.Query.IsStarted)
					await State.Query.Start();

				await Query.SetStatus(await State.ReportFilesNamespace.GetStringAsync(9, "Executing Query..."));

				await this.parsedReport.Execute(State);

				await State.Query.SetStatus(string.Empty);
			}
			catch (Exception ex)
			{
				await Query.LogMessage(ex);
				Log.Exception(ex);
			}
			finally
			{
				if (!Query.IsDone && !Query.IsAborted)
					await Query.Done();
			}
		}
	}
}
