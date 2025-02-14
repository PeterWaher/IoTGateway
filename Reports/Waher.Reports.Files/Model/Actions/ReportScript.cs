using System.Threading.Tasks;
using System.Xml;
using Waher.Script;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Executes script when report is being executed.
	/// </summary>
	public class ReportScript : ReportAction
	{
		private readonly Expression parsed;

		/// <summary>
		/// Executes script when report is being executed.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public ReportScript(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.parsed = new Expression(Xml.InnerText);
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			await this.parsed.EvaluateAsync(State.Variables);
			return true;
		}
	}
}
