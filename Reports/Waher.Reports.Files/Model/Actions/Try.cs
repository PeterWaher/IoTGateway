using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a TRY...CATCH...FINALLY statement in a report.
	/// </summary>
	public class Try : ReportAction
	{
		private readonly ReportAction[] @catch;
		private readonly ReportAction[] @finally;
		private readonly ReportAction[] actions;
		private readonly ReportStringAttribute variable;

		/// <summary>
		/// Defines a TRY...CATCH...FINALLY statement in a report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Try(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			this.actions = Report.ParseActions(Xml, out this.@catch, out this.@finally);
			this.variable = new ReportStringAttribute(Xml, "variable");
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public override async Task<bool> Execute(ReportState State)
		{
			try
			{
				foreach (ReportAction Action in this.actions)
					await Action.Execute(State);
			}
			catch (Exception ex)
			{
				if (this.@catch is null)
					ExceptionDispatchInfo.Capture(ex).Throw();

				string VariableName = await this.variable.Evaluate(State.Variables);
				if (string.IsNullOrEmpty(VariableName))
					VariableName = "ex";

				State.Variables[VariableName] = ex;

				foreach (ReportAction Action in this.@catch)
					await Action.Execute(State);
			}
			finally
			{
				if (!(this.@finally is null))
				{
					foreach (ReportAction Action in this.@finally)
						await Action.Execute(State);
				}
			}

			return true;
		}
	}
}
