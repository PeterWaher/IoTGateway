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
	}
}
