using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Includes an object into the report.
	/// </summary>
	public class ReportObject : ReportAction
	{
		private readonly ReportObjectAttribute content;

		/// <summary>
		/// Includes an object into the report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public ReportObject(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.content = new ReportObjectAttribute(Xml, null);
		}
	}
}
