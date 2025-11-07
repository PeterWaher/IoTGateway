using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Groups;
using Waher.Runtime.Collections;
using Waher.Things;

namespace Waher.Jobs.NodeTypes
{
	/// <summary>
	/// Abstract bast class for job tasks.
	/// </summary>
	public abstract class JobTaskNode : JobNode, IGroup
	{
		/// <summary>
		/// Abstract bast class for job tasks.
		/// </summary>
		public JobTaskNode()
		{
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Job);
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

				if (Child is IGroup Group)
					await Group.FindNodes(Nodes);
			}
		}

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <param name="Status">Execution status.</param>
		public abstract Task ExecuteTask(JobExecutionStatus Status);
	}
}
