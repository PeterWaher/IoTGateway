using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Inventory;
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
			return Task.FromResult(Parent is ConcentratorDevice || Parent is SourceNode || Parent is PartitionNode ||
				Parent is XmppBrokerNode);
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
		/// Gets the XMPP Client associated with node.
		/// </summary>
		/// <returns>XMPP Client</returns>
		/// <exception cref="Exception">If no XMPP Client could be found, associated with node.</exception>
		public XmppClient GetClient()
		{
			if (this.Parent is XmppBrokerNode BrokerNode)
				return BrokerNode.GetBroker().Client;
			else if (Types.TryGetModuleParameter("XMPP", out object Obj) && Obj is XmppClient Client)
				return Client;
			else
				throw new Exception("No XMPP Client associated with node.");
		}
	}
}
