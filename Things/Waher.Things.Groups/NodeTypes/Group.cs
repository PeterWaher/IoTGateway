using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;

namespace Waher.Things.Groups.NodeTypes
{
	/// <summary>
	/// Represents a group of nodes.
	/// </summary>
	public class Group : GroupNode, IGroup
	{
		/// <summary>
		/// Represents a group of nodes.
		/// </summary>
		public Group()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupSource), 3, "Group");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is NodeReference || 
				Child is MeteringNodeReference || Child is Group);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is Group);
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
				if (Child is IGroup Group)
					await Group.FindNodes(Nodes);
			}
		}

	}
}
