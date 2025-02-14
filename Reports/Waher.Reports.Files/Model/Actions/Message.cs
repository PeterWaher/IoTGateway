using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Things.Queries;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Reports a message.
	/// </summary>
	public class Message : ReportAction
	{
		private readonly ReportStringAttribute body;
		private readonly ReportEnumAttribute<QueryEventType> type;
		private readonly ReportEnumAttribute<QueryEventLevel> level;

		/// <summary>
		/// Reports a message.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Message(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.body = new ReportStringAttribute(Xml, null);
			this.type = new ReportEnumAttribute<QueryEventType>(Xml, "type");
			this.level = new ReportEnumAttribute<QueryEventLevel>(Xml, "level");
		}
	}
}
