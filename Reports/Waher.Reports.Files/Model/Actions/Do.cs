using System;
using System.Threading.Tasks;
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

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			while (true)
			{
				foreach (ReportAction Action in this.actions)
					await Action.Execute(State);

				object Result = await this.condition.EvaluateAsync(State.Variables);

				if (!(Result is bool ConditionResponse))
					throw new Exception("Condition did not return a Boolean value.");

				if (!ConditionResponse)
					break;
			}

			return true;
		}
	}
}
