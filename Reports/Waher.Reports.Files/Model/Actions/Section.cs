using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a section in the report.
	/// </summary>
	public class Section : ReportAction
	{
		private readonly ReportStringAttribute header;
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines a section in the report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Section(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.header = new ReportStringAttribute(Xml, "header");
			this.actions = Report.ParseActions(Xml);
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			string Header = await this.header.Evaluate(State.Variables);

			await State.Query.BeginSection(Header);

			try
			{
				foreach (ReportAction Action in this.actions)
					await Action.Execute(State);
			}
			finally
			{
				await State.Query.EndSection();
			}

			return true;
		}
	}
}
