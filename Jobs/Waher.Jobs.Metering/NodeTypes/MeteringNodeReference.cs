using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Groups;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.Metering;

namespace Waher.Jobs.Metering.NodeTypes
{
	/// <summary>
	/// A reference to a metering node.
	/// </summary>
	public class MeteringNodeReference : ReferenceNode, IGroup
	{
		/// <summary>
		/// A reference to a metering node.
		/// </summary>
		public MeteringNodeReference()
			: base()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(24, "Node ID:", 20)]
		[Page(66, "Job", 0)]
		[ToolTip(25, "ID of the node being referenced.")]
		[Required]
		public string ReferenceNodeId { get; set; }

		/// <summary>
		/// If child nodes should be included.
		/// </summary>
		[Header(26, "Include child nodes.", 30)]
		[Page(66, "Job", 0)]
		[ToolTip(27, "If child nodes should be included in the reference.")]
		public bool IncludeChildNodes { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringNodeReference), 28, "Metering Node Reference");
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		public override async Task FindNodes<T>(ChunkedList<T> Nodes)
		{
			INode Node = await MeteringTopology.GetNode(this.ReferenceNodeId);

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Node not found.");
				return;
			}

			if (Node is T TypedNode)
				Nodes.Add(TypedNode);
			else if (this.IncludeChildNodes)
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
							else
								CheckChildren.Add(Child);
						}
					}
				}
			}
		}
	}
}
