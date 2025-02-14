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
	}
}
