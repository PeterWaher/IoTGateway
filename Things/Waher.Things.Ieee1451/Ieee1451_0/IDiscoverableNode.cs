using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Interface for nodes that can be discovered on an IEEE 1451.0 network.
	/// </summary>
	public interface IDiscoverableNode : INode
	{
		/// <summary>
		/// A request for TEDS data has been received.
		/// </summary>
		/// <param name="DiscoveryMessage">Message</param>
		Task DiscoveryRequest(DiscoveryMessage DiscoveryMessage);
	}
}
