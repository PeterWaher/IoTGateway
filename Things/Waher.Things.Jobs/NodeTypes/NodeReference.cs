using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Groups;

namespace Waher.Things.Jobs.NodeTypes
{
	/// <summary>
	/// A reference to another node.
	/// </summary>
	public class NodeReference : JobNode, IGroup
	{
		/// <summary>
		/// A reference to another node.
		/// </summary>
		public NodeReference()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(20, "Node ID:", 0)]
		[Page(21, "Reference", 0)]
		[ToolTip(22, "Node ID of the node being referenced.")]
		[Required]
		public string ReferenceNodeId { get; set; }

		/// <summary>
		/// Source ID of node.
		/// </summary>
		[Header(23, "Source ID:", 0)]
		[Page(21, "Reference", 0)]
		[ToolTip(24, "Source ID of the node being referenced.")]
		[Required]
		public string ReferenceSourceId { get; set; }

		/// <summary>
		/// Partition of node.
		/// </summary>
		[Header(25, "Partition:", 0)]
		[Page(21, "Reference", 0)]
		[ToolTip(26, "Partition of the node being referenced.")]
		public string ReferencePartition { get; set; }

		/// <summary>
		/// If child nodes should be included.
		/// </summary>
		[Header(27, "Include child nodes.", 0)]
		[Page(21, "Reference", 0)]
		[ToolTip(28, "If child nodes should be included in the reference.")]
		public bool IncludeChildNodes { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupSource), 19, "Node Reference");
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
			return Task.FromResult(Parent is Job);
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		public async Task FindNodes<T>(ChunkedList<T> Nodes)
			where T : INode
		{
			if (!GroupSource.TryGetDataSource(this.ReferenceSourceId, out IDataSource Source))
			{
				await this.LogErrorAsync("InvalidReference", "Data Source not found.");
				return;
			}

			INode Node;

			if (string.IsNullOrEmpty(this.ReferencePartition))
			{
				Node = await Source.GetNodeAsync(new ThingReference(this.ReferenceNodeId,
					this.ReferenceSourceId));
			}
			else
			{
				Node = await Source.GetNodeAsync(new ThingReference(this.ReferenceNodeId,
					this.ReferenceSourceId, this.ReferencePartition));
			}

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Node not found.");
				return;
			}

			if (Node is T TypedNode)
				Nodes.Add(TypedNode);

			if (this.IncludeChildNodes)
			{
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
}
