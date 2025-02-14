using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports a table to be complete.
	/// </summary>
	public class TableComplete : ReportAction
	{
		private readonly ReportStringAttribute tableId;

		/// <summary>
		/// Reports a table to be complete.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public TableComplete(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.tableId = new ReportStringAttribute(Xml, "tableId");
		}
	}
}
