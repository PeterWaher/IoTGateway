using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;

namespace Waher.Things.Xmpp
{
	/// <summary>
	/// Abstract base class for devices on the XMPP network.
	/// </summary>
	public abstract class XmppDevice : ProvisionedMeteringNode
	{
		/// <summary>
		/// Abstract base class for devices on the XMPP network.
		/// </summary>
		public XmppDevice()
			: base()
		{
		}

		/// <summary>
		/// Tries to get the broker node, if one exists.
		/// </summary>
		/// <returns>Broker Node, if found, null otherwise.</returns>
		public Task<XmppBrokerNode> GetBrokerNode()
		{
			return this.GetAncestor<XmppBrokerNode>();
		}

		/// <summary>
		/// Gets the XMPP Client associated with node.
		/// </summary>
		/// <returns>XMPP Client</returns>
		/// <exception cref="Exception">If no XMPP Client could be found, associated with node.</exception>
		public async Task<XmppClient> GetClient()
		{
			XmppBrokerNode BrokerNode = await this.GetBrokerNode();

			if (!(BrokerNode is null))
				return (await BrokerNode.GetBroker()).Client;
			else if (Types.TryGetModuleParameter("XMPP", out XmppClient Client))
				return Client;
			else
				throw new Exception("No XMPP Client associated with node.");
		}
	}
}
