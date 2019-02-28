using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Alive message regularly sent on the network to inform members of the cluster
	/// of the status of the current machine.
	/// </summary>
	public class Alive : IClusterMessage
	{
		/// <summary>
		/// Machine status.
		/// </summary>
		public object Status
		{
			get;
			set;
		}

		/// <summary>
		/// Method called when the message has been received.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>If the message was accepted/processed or not.
		/// In Acknowledged service, this corresponds to ACK/NACK.</returns>
		public Task<bool> MessageReceived(ClusterEndpoint Endpoint, IPEndPoint RemoteEndpoint)
		{
			Endpoint.StatusReported(this.Status, RemoteEndpoint);
			return Task.FromResult<bool>(true);
		}
	}
}
