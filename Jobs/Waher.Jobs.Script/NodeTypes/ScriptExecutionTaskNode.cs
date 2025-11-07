using System;
using System.Text;
using System.Threading.Tasks;
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
		/// <summary>
		/// Sensor data readout task node.
		/// </summary>
		public ScriptExecutionTaskNode()
		{
		}

		/// <summary>
		/// Script to execute.
		/// </summary>
		[Header(1, "Script:", 0)]
		[Page(2, "Script", 0)]
		[ToolTip(3, "Script to execute.")]
		[ContentType("application/x-webscript")]
		public string[] Script { get; set; }

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
				StringBuilder sb = new StringBuilder();

				foreach (string s in this.Script)
					sb.AppendLine(s);

				await Expression.EvalAsync(sb.ToString(), Status.Variables);
				await Status.Job.RemoveErrorAsync("ScriptError");
			}
			catch (Exception ex)
			{
				await Status.Job.LogErrorAsync("ScriptError", ex.Message);
			}
		}
	}
}
