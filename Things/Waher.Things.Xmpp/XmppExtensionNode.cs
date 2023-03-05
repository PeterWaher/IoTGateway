using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Abstract base class for XMPP Extensions.
	/// </summary>
	public abstract class XmppExtensionNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Abstract base class for XMPP Extensions.
		/// </summary>
		public XmppExtensionNode()
			: base()
		{
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is XmppBrokerNode);
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
		/// Registers the extension with an XMPP Client.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public abstract Task RegisterExtension(XmppClient Client);

		/// <summary>
		/// Unregisters the extension from an XMPP Client.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public abstract Task UnregisterExtension(XmppClient Client);

		/// <summary>
		/// Checks if the extension has been registered on an XMPP Client.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <returns>If the extension has been registered.</returns>
		public abstract bool IsRegisteredExtension(XmppClient Client);

	}
}
