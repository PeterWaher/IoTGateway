using System.Xml;
using Waher.Content.Xml;
using Waher.Script;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a do-loop in a report.
	/// </summary>
	public class Do : ReportAction
	{
		private readonly Expression condition;
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines a while-loop in a report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Do(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.condition = new Expression(XML.Attribute(Xml, "condition"));
			this.actions = Report.ParseActions(Xml);
		}
	}
}
