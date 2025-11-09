using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Output;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Jobs.Metering.NodeTypes
{
	/// <summary>
	/// A reference to an output.
	/// </summary>
	public class OutputReference : ReferenceNode
	{
		/// <summary>
		/// A reference to an output.
		/// </summary>
		public OutputReference()
			: base()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(62, "Output ID:", 0)]
		[Page(23, "Reference", 100)]
		[ToolTip(63, "ID of the output being referenced.")]
		[Required]
		public string ReferenceNodeId { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupReference), 64, "Output Reference");
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		public override async Task FindNodes<T>(ChunkedList<T> Nodes)
		{
			INode Node = await OutputSource.GetNode(this.ReferenceNodeId);

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Output not found.");
				return;
			}

			if (Node is T TypedNode)
				Nodes.Add(TypedNode);
			else
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
