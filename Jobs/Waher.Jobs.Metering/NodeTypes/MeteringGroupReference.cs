using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Groups;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Jobs.Metering.NodeTypes
{
	/// <summary>
	/// A reference to a metering group.
	/// </summary>
	public class MeteringGroupReference : JobNode, IGroup
	{
		/// <summary>
		/// A reference to a metering group.
		/// </summary>
		public MeteringGroupReference()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(29, "Group ID:", 0)]
		[Page(23, "Reference", 100)]
		[ToolTip(30, "Group ID of the group being referenced.")]
		[Required]
		public string ReferenceNodeId { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringGroupReference), 31, "Metering Group Reference");
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
			INode Node = await GroupSource.GetNode(this.ReferenceNodeId);

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Group not found.");
				return;
			}

			if (Node is T TypedNode)
				Nodes.Add(TypedNode);

			ChunkedList<INode> CheckChildren = new ChunkedList<INode>() { Node };

			while (CheckChildren.HasFirstItem)
			{
				Node = CheckChildren.RemoveFirst();
				IEnumerable<INode> Children = await Node.ChildNodes;

				if (!(Children is null))
				{
					foreach (INode Child in Children)
					{
						if (Child is T TypedChild)
							Nodes.Add(TypedChild);

						CheckChildren.Add(Child);
					}
				}
			}
		}
	}
}
