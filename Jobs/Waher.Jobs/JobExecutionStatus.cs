using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Jobs.NodeTypes;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Queries;

namespace Waher.Jobs
{
	/// <summary>
	/// Contains information about the execution of a job.
	/// </summary>
	public class JobExecutionStatus : IDisposable
	{
		private readonly DateTime startTime = DateTime.UtcNow;
		private readonly Variables variables = new Variables();
		private readonly Query query;
		private readonly Language language;
		private readonly JobReportDetail reportDetail;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		/// <summary>
		/// Contains information about the execution of a job.
		/// </summary>
		/// <param name="Job">Job being executed.</param>
		/// <param name="Query">Optional Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		/// <param name="ReportDetail">How much detail to include in the report.</param>
		public JobExecutionStatus(Job Job, Query Query, Language Language,
			JobReportDetail ReportDetail)
		{
			this.Job = Job;
			this.query = Query;
			this.language = Language;
			this.reportDetail = ReportDetail;
		}

		/// <summary>
		/// Job being executed.
		/// </summary>
		public Job Job { get; }

		/// <summary>
		/// Optional Query data receptor.
		/// </summary>
		public Query Query => this.query;

		/// <summary>
		/// Language to use.
		/// </summary>
		public Language Language => this.language;

		/// <summary>
		/// How much detail to include in the report.
		/// </summary>
		public JobReportDetail ReportDetail => this.reportDetail;

		/// <summary>
		/// Start-time of job execution.
		/// </summary>
		public DateTime StartTime => this.startTime;

		/// <summary>
		/// Job execution state variables.
		/// </summary>
		public Variables Variables => this.variables;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>"/>
		/// </summary>
		public void Dispose()
		{
			this.semaphore.Dispose();
		}

		/// <summary>
		/// Locks the job status object.
		/// </summary>
		public async Task Lock()
		{
			await this.semaphore.WaitAsync();
		}

		/// <summary>
		/// Unlocks the job status object.
		/// </summary>
		public void Unlock()
		{
			this.semaphore.Release();
		}
	}
}
