using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Reports.Files.Commands;
using Waher.Reports.Files.Model;

namespace Waher.Reports.Files
{
	/// <summary>
	/// Report node based on the contents of a report file.
	/// </summary>
	public class ReportFileNode : ReportNode
	{
		private readonly string fileName;

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
		/// Gets the command object to execute a report. If null is returned, the report
		/// node is just a group placeholder of sub-reports.
		/// </summary>
		/// <returns>Command object, or null if node is only a placeholder.</returns>
		public override Task<ExecuteReport> GetExecuteCommand()
		{
			try
			{
				ReportFile Report = new ReportFile(this.fileName);
				ExecuteReportFileCommand Command = new ExecuteReportFileCommand(Report, this);

				return Task.FromResult<ExecuteReport>(Command);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				return Task.FromResult<ExecuteReport>(null);
			}
		}
	}
}
