using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports a status message.
	/// </summary>
	public class Status : ReportAction
	{
		private readonly ReportStringAttribute body;

		/// <summary>
		/// Reports a status message.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Status(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.body = new ReportStringAttribute(Xml, null);
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			string Body = await this.body.Evaluate(State.Variables);

			await State.Query.SetStatus(Body);

			return true;
		}
	}
}
