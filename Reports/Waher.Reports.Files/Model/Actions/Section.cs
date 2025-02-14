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
	}
}
