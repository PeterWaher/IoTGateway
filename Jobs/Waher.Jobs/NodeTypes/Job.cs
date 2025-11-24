using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Groups;
using Waher.Jobs.Commands;
using Waher.Persistence.Attributes;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Queries;

namespace Waher.Jobs.NodeTypes
{
	/// <summary>
	/// Represents a job.
	/// </summary>
	public class Job : JobNode, IGroup
	{
		private DateTime? executionTime = null;
		private Duration? period = null;
		private Duration? burstInterval = null;
		private DateTime? expires = null;
		private int burstCount = 1;

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
			set => this.executionTime = value;
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
		/// Gets or sets the scheduled execution time of the job.
		/// </summary>
		[Header(15, "Expires:", 40)]
		[Page(2, "Job", 0)]
		[ToolTip(16, "Repetitive job is not scheduled after this point in time.")]
		[DefaultValueNull]
		public DateTime? Expires
		{
			get => this.expires;
			set => this.expires = value;
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		protected override Task NodeUpdated()
		{
			return this.NodeUpdated(true);
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		/// <param name="ScheduleJob">If job should be (re)schduled.</param>
		private async Task NodeUpdated(bool ScheduleJob)
		{
			await base.NodeUpdated();

			if (ScheduleJob)
				_ = Task.Run(async () => await JobScheduler.Schedule(this, true));
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
		/// Job execution time is updated from the scheduler.
		/// </summary>
		/// <param name="ExecutionTime">New execution time.</param>
		internal async Task ExecutionTimeUpdated(DateTime? ExecutionTime)
		{
			this.executionTime = ExecutionTime;
			await this.NodeUpdated(false);
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Parameters = (LinkedList<Parameter>)await base.GetDisplayableParametersAsync(Language, Caller);

			if (this.executionTime.HasValue)
			{
				Parameters.AddLast(new DateTimeParameter("ExecutionTime",
					await Language.GetStringAsync(typeof(Job), 11, "Execution Time"),
					this.executionTime.Value));
			}

			if (this.period.HasValue)
			{
				Parameters.AddLast(new DurationParameter("Period",
					await Language.GetStringAsync(typeof(Job), 12, "Period"),
					this.period.Value));
			}

			if (this.burstInterval.HasValue)
			{
				Parameters.AddLast(new DurationParameter("BurstInterval",
					await Language.GetStringAsync(typeof(Job), 13, "Burst Interval"),
					this.burstInterval.Value));

				Parameters.AddLast(new Int32Parameter("BurstCount",
					await Language.GetStringAsync(typeof(Job), 14, "Burst Count"),
					this.burstCount));
			}

			if (this.expires.HasValue)
			{
				Parameters.AddLast(new DateTimeParameter("Expires",
					await Language.GetStringAsync(typeof(Job), 17, "Expires"),
					this.expires.Value));
			}

			return Parameters;
		}

		/// <summary>
		/// Node has been added to the root node.
		/// </summary>
		/// <param name="Root">Root node.</param>
		public override Task AddedToRoot(Root Root)
		{
			return JobScheduler.Schedule(this, true);
		}

		/// <summary>
		/// Node has been removed from the root node.
		/// </summary>
		/// <param name="Root">Root node.</param>
		public override Task RemovedFromRoot(Root Root)
		{
			return JobScheduler.Remove(this);
		}
	}
}
