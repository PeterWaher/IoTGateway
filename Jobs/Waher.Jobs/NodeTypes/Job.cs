using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Groups;
using Waher.Jobs.Commands;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Jobs.NodeTypes
{
	/// <summary>
	/// Represents a job.
	/// </summary>
	public class Job : JobNode, IGroup
	{
		/// <summary>
		/// Represents a job.
		/// </summary>
		public Job()
		{
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

	}
}
