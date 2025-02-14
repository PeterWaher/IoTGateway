using System.Collections;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Script;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a for each-loop in a report.
	/// </summary>
	public class ForEach : ReportAction
	{
		private readonly Expression set;
		private readonly ReportStringAttribute variable;
		private readonly ReportAction[] actions;

		/// <summary>
		/// Defines a for each-loop in a report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public ForEach(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.set = new Expression(XML.Attribute(Xml, "set"));
			this.variable = new ReportStringAttribute(Xml, "variable");
			this.actions = Report.ParseActions(Xml);
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			object Set = await this.set.EvaluateAsync(State.Variables);
			string Variable = await this.variable.Evaluate(State.Variables);

			if (!(Set is IEnumerable SetEnum))
				SetEnum = new object[] { Set };

			foreach (object Item in SetEnum)
			{
				State.Variables[Variable] = Item;

				foreach (ReportAction Action in this.actions)
					await Action.Execute(State);
			}

			return true;
		}
	}
}
