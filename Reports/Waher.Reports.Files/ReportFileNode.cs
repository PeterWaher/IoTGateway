using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Reports.Files.Commands;
using Waher.Reports.Files.Model;
using Waher.Script;
using Waher.Things;

namespace Waher.Reports.Files
{
	/// <summary>
	/// Report node based on the contents of a report file.
	/// </summary>
	public class ReportFileNode : ReportNode
	{
		private readonly string fileName;
		private ReportFile parsedReport = null;
		private DateTime reportTimestamp = DateTime.MinValue;

		/// <summary>
		/// Report node based on the contents of a report file.
		/// </summary>
		/// <param name="FileName">Report file name.</param>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Parent">Parent node.</param>
		public ReportFileNode(string FileName, string NodeId, ReportNode Parent)
			: base(NodeId, Parent)
		{
			this.fileName = Path.GetFullPath(FileName);
		}

		/// <summary>
		/// Report file name.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Parsed report, or null if unable to parse report.
		/// </summary>
		public ReportFile ParsedReport
		{
			get
			{
				try
				{
					if (this.parsedReport is null || File.GetLastWriteTimeUtc(this.fileName) > this.reportTimestamp)
					{
						this.parsedReport = new ReportFile(this.fileName);
						this.reportTimestamp = File.GetLastWriteTimeUtc(this.fileName);
					}

					return this.parsedReport;
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					return null;
				}
			}
		}

		/// <summary>
		/// Gets the command object to execute a report. If null is returned, the report
		/// node is just a group placeholder of sub-reports.
		/// </summary>
		/// <returns>Command object, or null if node is only a placeholder.</returns>
		public override Task<ExecuteReport> GetExecuteCommand()
		{
			ReportFile Report = this.ParsedReport;
			if (Report is null)
				return Task.FromResult<ExecuteReport>(null);

			ExecuteReportFileCommand Command = new ExecuteReportFileCommand(Report, this, new Variables());
			return Task.FromResult<ExecuteReport>(Command);
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		public override Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			ReportFile Report = this.ParsedReport;
			if (Report is null)
				return Task.FromResult(false);

			if (Report.Privileges.Length == 0)
				return Task.FromResult(true);

			foreach (string Privilege in Report.Privileges)
			{
				if (!Caller.HasPrivilege(Privilege))
					return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}
	}
}
