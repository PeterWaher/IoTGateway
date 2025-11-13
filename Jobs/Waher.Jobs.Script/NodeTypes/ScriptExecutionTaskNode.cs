using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Jobs.NodeTypes;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Jobs.Script.NodeTypes
{
	/// <summary>
	/// Sensor data readout task node.
	/// </summary>
	public class ScriptExecutionTaskNode : JobTaskNode
	{
		private string[] script;
		private Expression parsedScript;

		/// <summary>
		/// Sensor data readout task node.
		/// </summary>
		public ScriptExecutionTaskNode()
		{
		}

		/// <summary>
		/// Script to execute.
		/// </summary>
		[Header(1, "Script:", 10)]
		[Page(2, "Job", 0)]
		[ToolTip(3, "Script to execute.")]
		[ContentType("application/x-webscript")]
		public string[] Script 
		{
			get => this.script;
			set
			{
				this.script = value;
				this.parsedScript = null;
			}
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ScriptExecutionTaskNode), 4, "Script Execution Task");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <param name="Status">Execution status.</param>
		public override async Task ExecuteTask(JobExecutionStatus Status)
		{
			try
			{
				if (Status.ReportDetail != JobReportDetail.None)
				{
					await Status.Query.Start();
					await Status.Query.SetTitle(this.NodeId);
				}

				if (this.parsedScript is null)
				{
					StringBuilder sb = new StringBuilder();

					foreach (string s in this.Script)
						sb.AppendLine(s);

					this.parsedScript = new Expression(sb.ToString());
				}

				DateTime Start = DateTime.UtcNow;
				object Result = await this.parsedScript.EvaluateAsync(Status.Variables);
				TimeSpan ExecutionTime = DateTime.UtcNow.Subtract(Start);

				if (Status.ReportDetail == JobReportDetail.Details)
				{
					if (!(Result is null))
						await Status.Query.NewObject(Result);
				}

				if (Status.ReportDetail != JobReportDetail.None)
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Execution time: **");
					sb.Append(MarkdownDocument.Encode(ExecutionTime.ToString()));
					sb.Append("**");

					await Status.Query.NewObject(new MarkdownContent(sb.ToString()));
				}

				await Status.Job.RemoveErrorAsync("ScriptError");
			}
			catch (Exception ex)
			{
				await Status.Job.LogErrorAsync("ScriptError", ex.Message);
			}
			finally
			{
				if (Status.ReportDetail != JobReportDetail.None)
					await Status.Query.Done();
			}
		}
	}
}
