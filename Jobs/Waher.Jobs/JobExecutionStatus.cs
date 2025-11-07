using System;
using Waher.Jobs.NodeTypes;
using Waher.Script;

namespace Waher.Jobs
{
	/// <summary>
	/// Contains information about the execution of a job.
	/// </summary>
	public class JobExecutionStatus
	{
		private readonly DateTime startTime = DateTime.UtcNow;
		private readonly Variables variables = new Variables();

		/// <summary>
		/// Contains information about the execution of a job.
		/// </summary>
		/// <param name="Job">Job being executed.</param>
		public JobExecutionStatus(Job Job)
		{
			this.Job = Job;
		}

		/// <summary>
		/// Job being executed.
		/// </summary>
		public Job Job { get; }

		/// <summary>
		/// Start-time of job execution.
		/// </summary>
		public DateTime StartTime => this.startTime;

		/// <summary>
		/// Job execution state variables.
		/// </summary>
		public Variables Variables => this.variables;
	}
}
