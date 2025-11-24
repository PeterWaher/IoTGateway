using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Processors;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Jobs.Metering.NodeTypes
{
	/// <summary>
	/// A reference to a processor.
	/// </summary>
	public class ProcessorReference : ReferenceNode
	{
		/// <summary>
		/// A reference to a processor.
		/// </summary>
		public ProcessorReference()
			: base()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(59, "Processor ID:", 20)]
		[Page(66, "Job", 0)]
		[ToolTip(60, "ID of the processor being referenced.")]
		[Required]
		public string ReferenceNodeId { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupReference), 61, "Processor Reference");
		}

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		public override async Task FindNodes<T>(ChunkedList<T> Nodes)
		{
			INode Node = await ProcessorSource.GetNode(this.ReferenceNodeId);

			if (Node is null)
			{
				await this.LogErrorAsync("InvalidReference", "Processor not found.");
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
