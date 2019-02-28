using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Message sent when an endpoint is shut down, to let the other endpoints know
	/// the current endpoint is no longer available.
	/// </summary>
	public class ShuttingDown : IClusterMessage
	{
		/// <summary>
		/// Message sent when an endpoint is shut down, to let the other endpoints know
		/// the current endpoint is no longer available.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>If the message was accepted/processed or not.
		/// In Acknowledged service, this corresponds to ACK/NACK.</returns>
		public Task<bool> MessageReceived(ClusterEndpoint Endpoint, IPEndPoint RemoteEndpoint)
		{
			Endpoint.EndpointShutDown(RemoteEndpoint);
			return Task.FromResult<bool>(true);
		}
	}
}
