using System.Xml;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a TRY...CATCH...FINALLY statement in a report.
	/// </summary>
	public class Try : ReportAction
	{
		private readonly ReportAction[] @catch;
		private readonly ReportAction[] @finally;
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines a TRY...CATCH...FINALLY statement in a report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Try(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.actions = Report.ParseActions(Xml, out this.@catch, out this.@finally);
		}
	}
}
