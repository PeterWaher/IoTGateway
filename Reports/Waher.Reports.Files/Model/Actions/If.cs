using System.Xml;
using Waher.Content.Xml;
using Waher.Script;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines actions that will be executed if a condition is met.
	/// </summary>
	public class If : ReportAction
	{
		private readonly Expression condition;
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines actions that will be executed if a condition is met.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public If(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.condition = new Expression(XML.Attribute(Xml, "condition"));
			this.actions = Report.ParseActions(Xml);
		}
	}
}
