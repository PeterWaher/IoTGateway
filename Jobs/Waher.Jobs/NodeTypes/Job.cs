using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Groups;
using Waher.Jobs.Commands;
using Waher.Persistence.Attributes;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.Queries;

namespace Waher.Jobs.NodeTypes
{
	/// <summary>
	/// Represents a job.
	/// </summary>
	public class Job : JobNode, IGroup
	{
		private DateTime? scheduledExecutionTime = null;
		private DateTime? executionTime = null;
		private Duration? period = null;
		private Duration? burstInterval = null;
		private int burstCount = 1;
		private int burstCountLeft = 1;

		/// <summary>
		/// Represents a job.
		/// </summary>
		public Job()
		{
		}

		/// <summary>
		/// Gets or sets the scheduled execution time of the job.
		/// </summary>
		[Header(3, "Scheduled execution time:", 10)]
		[Page(2, "Job", 0)]
		[ToolTip(4, "When the job is scheduled to execute.")]
		[DefaultValueNull]
		public DateTime? ExecutionTime
		{
			get => this.executionTime;
			set
			{
				if (this.executionTime != value)
				{
					this.executionTime = value;
					Task.Run(async () => await this.RescheduleJobDelayed(1000));
				}
			}
		}

		/// <summary>
		/// Gets or sets the scheduled execution time of the job.
		/// </summary>
		[Header(5, "Job Period:", 20)]
		[Page(2, "Job", 0)]
		[ToolTip(6, "Time between job executions. Defines a repetitive job.")]
		[DefaultValueNull]
		public Duration? Period
		{
			get => this.period;
			set => this.period = value;
		}

		/// <summary>
		/// Number of executions each period.
		/// </summary>
		[Header(7, "Burst count:", 30)]
		[Page(2, "Job", 0)]
		[ToolTip(8, "Number of times the job will be executed each period.")]
		[DefaultValue(1)]
		public int BurstCount
		{
			get => this.burstCount;
			set => this.burstCount = value;
		}

		/// <summary>
		/// Gets or sets the scheduled execution time of the job.
		/// </summary>
		[Header(9, "Burst Interval:", 40)]
		[Page(2, "Job", 0)]
		[ToolTip(10, "Time between job executions in a burst.")]
		[DefaultValueNull]
		public Duration? BurstInterval
		{
			get => this.burstInterval;
			set => this.burstInterval = value;
		}

		/// <summary>
		/// Number of executions each period.
		/// </summary>
		[Header(11, "Burst count left:", 50)]
		[Page(2, "Job", 0)]
		[ToolTip(12, "Number of times left the job will be executed within the current period.")]
		[DefaultValue(1)]
		public int BurstCountLeft
		{
			get => this.burstCountLeft;
			set => this.burstCountLeft = value;
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Job), 2, "Job");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is JobTaskNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		public async Task<T[]> FindNodes<T>()
			where T : INode
		{
			ChunkedList<T> Nodes = new ChunkedList<T>();
			await this.FindNodes(Nodes);
			return Nodes.ToArray();
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		public async Task FindNodes<T>(ChunkedList<T> Nodes)
			where T : INode
		{
			IEnumerable<INode> Children = await this.ChildNodes;
			if (Children is null)
				return;

			foreach (INode Child in Children)
			{
				if (Child is T Typed)
					Nodes.Add(Typed);
				else if (Child is IGroup Group)
					await Group.FindNodes(Nodes);
			}
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			ChunkedList<ICommand> Commands = new ChunkedList<ICommand>
			{
				new ExecuteJob(this)
			};

			Commands.AddRange(await base.Commands);

			return Commands.ToArray();
		}

		/// <summary>
		/// Executes the job.
		/// </summary>
		public Task ExecuteJob()
		{
			return this.ExecuteJob(null);
		}

		/// <summary>
		/// Executes the job.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task ExecuteJob(Language Language)
		{
			return this.ExecuteJob(null, Language, JobReportDetail.None);
		}

		/// <summary>
		/// Executes the job.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		/// <param name="ReportDetail">How much detail to include in the report.</param>
		public async Task ExecuteJob(Query Query, Language Language,
			JobReportDetail ReportDetail)
		{
			Language ??= await Translator.GetDefaultLanguageAsync();

			using JobExecutionStatus State = new JobExecutionStatus(this, Query, Language, ReportDetail);
			JobTaskNode[] Tasks = await this.FindNodes<JobTaskNode>();

			foreach (JobTaskNode Task in Tasks)
				await Task.ExecuteTask(State);
		}

		/// <summary>
		/// Reschedules the job, with a small delay.
		/// </summary>
		/// <param name="DelayMs">Milliseconds to delay the rescheduling of the job.</param>
		public async Task RescheduleJobDelayed(int DelayMs)
		{
			await Task.Delay(DelayMs);
			await this.RescheduleJob();
		}

		/// <summary>
		/// Reschedules a job.
		/// </summary>
		public async Task RescheduleJob()
		{
			try
			{
				if (this.scheduledExecutionTime.HasValue)
				{
					JobScheduler.Scheduler?.Remove(this.scheduledExecutionTime.Value);
					this.scheduledExecutionTime = null;
				}

				if (!this.executionTime.HasValue)
					return;

				DateTime Now = DateTime.UtcNow;
				bool Changed = false;

				while (this.executionTime.Value.ToUniversalTime() < Now &&
					await this.AdvanceToNextTime(false))
				{
					Changed = true;
				}

				if (Changed)
					await this.UpdateAsync();

				if (this.executionTime.HasValue)
				{
					this.scheduledExecutionTime = JobScheduler.Scheduler?.Add(
						this.executionTime.Value, this.ExecuteScheduledJob, null);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task ExecuteScheduledJob(object _)
		{
			try
			{
				await this.ExecuteJob(await Translator.GetDefaultLanguageAsync());
				await this.RemoveErrorAsync("ExecutionError");
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync("ExecutionError", ex.Message);
			}
			finally
			{
				await this.AdvanceToNextTime(true);
			}
		}

		private async Task<bool> AdvanceToNextTime(bool PersistChange)
		{
			if (!this.executionTime.HasValue)
				return false;

			DateTime TP = this.executionTime.Value;

			if (this.burstInterval.HasValue &&
				this.burstInterval.Value > Duration.Zero)
			{
				if (this.burstCountLeft > 0)
				{
					this.executionTime = TP + this.burstInterval.Value;
					this.burstCountLeft--;

					if (PersistChange)
						await this.NodeUpdated();

					return true;
				}
				else if (this.period.HasValue && this.period.Value > Duration.Zero)
				{
					this.burstCountLeft = this.burstCount;
					this.executionTime = TP - this.burstCount * this.burstInterval.Value;
				}
				else
					return false;
			}

			if (this.period.HasValue && this.period.Value > Duration.Zero)
			{
				this.executionTime += this.period.Value;

				if (PersistChange)
					await this.NodeUpdated();

				return true;
			}

			this.executionTime = null;

			if (PersistChange)
				await this.NodeUpdated();

			return false;
		}
	}
}
