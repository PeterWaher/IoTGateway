using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Jobs.NodeTypes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Threading;
using Waher.Runtime.Timing;
using Waher.Things;

namespace Waher.Jobs
{
	/// <summary>
	/// Module that manages the scheduling and execution of jobs.
	/// </summary>
	public class JobScheduler : IModule
	{
		private static Scheduler scheduler = null;
		private bool disposeScheduler = false;

		/// <summary>
		/// Module that manages the scheduling and execution of jobs.
		/// </summary>
		public JobScheduler()
		{
		}

		/// <summary>
		/// Scheduler used to schedule jobs.
		/// </summary>
		public static Scheduler Scheduler => scheduler;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			if (scheduler is null)
			{
				if (Types.TryGetModuleParameter("Scheduler", out scheduler))
					this.disposeScheduler = false;
				else
				{
					scheduler = new Scheduler();
					this.disposeScheduler = true;
				}
			}

			foreach (INode Node in await JobSource.Root.ChildNodes)
			{
				if (Node is Job Job)
					await Schedule(Job, false);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (this.disposeScheduler)
				scheduler?.Dispose();

			scheduler = null;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Checks the schedule of a job.
		/// </summary>
		/// <param name="Job">Job</param>
		/// <param name="OnlyIfDifferent">Reschedule only if parameters different.</param>
		public static async Task Schedule(Job Job, bool OnlyIfDifferent)
		{
			using Semaphore Semaphore = await Semaphores.BeginWrite("Jobs." + Job.NodeId);
			JobSchedule Schedule;

			if (!Job.ExecutionTime.HasValue)
				return;

			lock (scheduledJobs)
			{
				if (scheduledJobs.TryGetValue(Job.NodeId, out Schedule))
				{
					if (OnlyIfDifferent &&
						Job.ExecutionTime == Schedule.ExecutionTime &&
						Job.Period == Schedule.Period &&
						Job.BurstInterval == Schedule.BurstInterval &&
						Job.BurstCount == Schedule.BurstCount &&
						Schedule.ScheduledExecutionTime.HasValue)
					{
						return;
					}

					if (Schedule.ScheduledExecutionTime.HasValue)
					{
						scheduler.Remove(Schedule.ScheduledExecutionTime.Value);
						Schedule.ScheduledExecutionTime = null;
					}

					Schedule.Job = Job;
					Schedule.ExecutionTime = Job.ExecutionTime;
					Schedule.Period = Job.Period;
					Schedule.BurstInterval = Job.BurstInterval;
					Schedule.BurstCount = Job.BurstCount;
					Schedule.BurstCountLeft = Job.BurstCount;
				}
				else
				{
					scheduledJobs[Job.NodeId] = Schedule = new JobSchedule()
					{
						Job = Job,
						ExecutionTime = Job.ExecutionTime,
						Period = Job.Period,
						BurstInterval = Job.BurstInterval,
						BurstCount = Job.BurstCount,
						BurstCountLeft = Job.BurstCount
					};
				}
			}

			await Schedule.Next(true);
		}

		private static readonly Dictionary<string, JobSchedule> scheduledJobs = new Dictionary<string, JobSchedule>();

		private class JobSchedule
		{
			public Job Job;
			public DateTime? ScheduledExecutionTime = null;
			public DateTime? ExecutionTime = null;
			public Duration? Period = null;
			public Duration? BurstInterval = null;
			public int BurstCount = 1;
			public int BurstCountLeft = 1;

			public async Task<DateTime?> GetNextFreeTime(bool Reschedule)
			{
				if (!this.ExecutionTime.HasValue)
					return null;

				DateTime TP = this.ExecutionTime.Value;
				DateTime Now = DateTime.UtcNow;

				if (!this.Period.HasValue && !this.BurstInterval.HasValue)
				{
					if (Reschedule)
					{
						if (TP.ToUniversalTime() < Now)
							return Now;
						else
							return TP;
					}
					else
					{
						await this.Job.ExecutionTimeUpdated(null);
						return null;
					}
				}

				bool Updated = false;

				do
				{
					if (this.BurstInterval.HasValue)
					{
						if (this.BurstCountLeft < this.BurstCount)
							TP += (this.BurstCount - this.BurstCountLeft) * this.BurstInterval.Value;

						while (
							this.BurstCountLeft > 0 &&
							TP.ToUniversalTime() < Now)
						{
							TP += this.BurstInterval.Value;
							this.BurstCountLeft--;
						}
					}

					if (TP.ToUniversalTime() >= Now)
					{
						if (Updated)
							await this.Job.ExecutionTimeUpdated(TP);

						return TP;
					}

					if (!this.Period.HasValue || this.Period.Value <= Duration.Zero)
					{
						await this.Job.ExecutionTimeUpdated(null);
						return null;
					}

					TP = this.ExecutionTime.Value + this.Period.Value;
					this.ExecutionTime = TP;
					this.BurstCountLeft = this.BurstCount;
					Updated = true;
				}
				while (TP.ToUniversalTime() < Now);

				await this.Job.ExecutionTimeUpdated(TP);

				return TP;
			}

			public async Task ExecuteJob(object _)
			{
				using Semaphore Semaphore = await Semaphores.BeginWrite("Jobs." + this.Job.NodeId);

				try
				{
					await this.Job.ExecuteJob(await Translator.GetDefaultLanguageAsync());
					await this.Job.RemoveErrorAsync("ExecutionError");
				}
				catch (Exception ex)
				{
					await this.Job.LogErrorAsync("ExecutionError", ex.Message);
				}
				finally
				{
					await this.Next(false);
				}
			}

			public async Task Next(bool Reschedule)
			{
				DateTime? Next = await this.GetNextFreeTime(Reschedule);

				if (Next.HasValue)
					this.ScheduledExecutionTime = scheduler?.Add(Next.Value, this.ExecuteJob, null);
				else
				{
					this.ScheduledExecutionTime = null;

					lock (scheduledJobs)
					{
						scheduledJobs.Remove(this.Job.NodeId);
					}
				}
			}
		}

		/// <summary>
		/// Removes any scheduled activity for the job.
		/// </summary>
		/// <param name="Job">Job</param>
		/// <returns>If a scheduled event was found and removed.</returns>
		public static async Task<bool> Remove(Job Job)
		{
			using Semaphore Semaphore = await Semaphores.BeginWrite("Jobs." + Job.NodeId);
			
			lock (scheduledJobs)
			{
				if (!scheduledJobs.TryGetValue(Job.NodeId, out JobSchedule Schedule))
					return false;

				if (Schedule.ScheduledExecutionTime.HasValue)
				{
					scheduler.Remove(Schedule.ScheduledExecutionTime.Value);
					Schedule.ScheduledExecutionTime = null;
				}

				return scheduledJobs.Remove(Job.NodeId);
			}
		}
	}
}
