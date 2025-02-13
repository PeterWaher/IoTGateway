using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.Reports
{
	/// <summary>
	/// A folder of reports.
	/// </summary>
	public class ReportsFolder : ReportNode
	{
		/// <summary>
		/// A folder of reports.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Parent">Parent Node, or null if a root node.</param>
		public ReportsFolder(string NodeId, ReportNode Parent)
			: base(NodeId, Parent)
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ReportsDataSource), 2, "Report Folder");
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<ExecuteReport> GetExecuteCommand()
		{
			return Task.FromResult<ExecuteReport>(null);
		}
	}
}
