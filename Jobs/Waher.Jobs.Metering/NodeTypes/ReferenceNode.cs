using System;
using System.Threading.Tasks;
using Waher.Groups;
using Waher.Runtime.Collections;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Jobs.Metering.NodeTypes
{
	/// <summary>
	/// Abstract base class for reference nodes.
	/// </summary>
	public abstract class ReferenceNode : JobNode, IGroup
	{
		/// <summary>
		/// Abstract base class for reference nodes.
		/// </summary>
		public ReferenceNode()
			: base()
		{
			this.NodeId = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(65, "Label:", 10)]
		[Page(66, "Job", 0)]
		[ToolTip(67, "Label presenting the node in the job source.")]
		[Required]
		public string Label { get; set; }

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public override string LocalId => string.IsNullOrEmpty(this.Label) ? this.NodeId : this.Label;

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		public override string LogId => this.LocalId;

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
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is SensorDataReadoutTaskNode);
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
		public abstract Task FindNodes<T>(ChunkedList<T> Nodes)
			where T : INode;
	}
}
