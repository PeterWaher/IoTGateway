using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Groups;
using Waher.Things.Jobs.NodeTypes.Jobs;

namespace Waher.Things.Jobs.NodeTypes.References
{
	/// <summary>
	/// A reference to a group node.
	/// </summary>
	public class GroupNodeReference : JobNode, IGroup
	{
		/// <summary>
		/// A reference to a group node.
		/// </summary>
		public GroupNodeReference()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(30, "Group ID:", 0)]
		[Page(21, "Reference", 0)]
		[ToolTip(31, "ID of the group being referenced.")]
		[Required]
		public string ReferenceGroupId { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupSource), 32, "Group Reference");
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
		public async Task FindNodes<T>(ChunkedList<T> Nodes)
			where T : INode
		{
			INode Node = await GroupSource.GetNode(this.ReferenceGroupId);

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Group not found.");
				return;
			}

			if (Node is IGroup Group)
				await Group.FindNodes(Nodes);
		}
	}
}
