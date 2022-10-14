using System.Threading.Tasks;
using Waher.Things.Attributes;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Abstract base class for nodes.
	/// </summary>
	public abstract class XmppNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Abstract base class for nodes.
		/// </summary>
		public XmppNode()
			: base()
		{
		}

		/// <summary>
		/// Node ID
		/// </summary>
		[Page(2, "XMPP", 100)]
		[Header(10, "Node ID:")]
		[ToolTip(11, "Node ID in data source (and partition).")]
		public string RemoteNodeID { get; set; }

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is ConcentratorDevice || Parent is SourceNode || Parent is PartitionNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

	}
}
