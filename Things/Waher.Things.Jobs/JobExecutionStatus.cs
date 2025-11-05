using System;
using System.Threading.Tasks;
using Waher.Things.Jobs.NodeTypes.Jobs;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Jobs
{
	/// <summary>
	/// Contains information about the execution of a job.
	/// </summary>
	public class JobExecutionStatus
	{
		private readonly DateTime startTime = DateTime.UtcNow;

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
		/// Metering nodes involved in the job execution.
		/// </summary>
		public IMeteringNode[] MeteringNodes { get; set; }

		/// <summary>
		/// Start-time of job execution.
		/// </summary>
		public DateTime StartTime => this.startTime;
	}
}
