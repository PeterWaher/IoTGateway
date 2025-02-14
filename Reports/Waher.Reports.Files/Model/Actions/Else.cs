using System.Threading.Tasks;
using System.Xml;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines actions that will be executed if no conditions are met.
	/// </summary>
	public class Else : ReportAction
	{
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines actions that will be executed if no conditions are met.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Else(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.actions = Report.ParseActions(Xml);
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			foreach (ReportAction Action in this.actions)
				await Action.Execute(State);

			return true;
		}
	}
}
