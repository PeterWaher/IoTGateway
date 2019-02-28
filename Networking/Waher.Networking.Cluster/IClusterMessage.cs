using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Interface for cluster messages.
	/// </summary>
	public interface IClusterMessage : IClusterObject
	{
		/// <summary>
		/// Method called when the message has been received.
		/// </summary>
		/// <param name="LocalEndpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>If the message was accepted/processed or not.
		/// In Acknowledged service, this corresponds to ACK/NACK.</returns>
		Task<bool> MessageReceived(ClusterEndpoint LocalEndpoint, IPEndPoint RemoteEndpoint);
	}
}
