using System.Threading.Tasks;
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

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			QueryEventType Type;
			QueryEventLevel Level;
			string Body = await this.body.Evaluate(State.Variables);

			if (this.type.IsEmpty)
				Type = QueryEventType.Information;
			else
				Type = await this.type.Evaluate(State.Variables);

			if (this.level.IsEmpty)
				Level = QueryEventLevel.Minor;
			else
				Level = await this.level.Evaluate(State.Variables);

			await State.Query.LogMessage(Type, Level, Body);

			return true;
		}
	}
}
