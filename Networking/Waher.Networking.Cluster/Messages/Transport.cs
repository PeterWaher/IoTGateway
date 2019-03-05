using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Transports a message, as part of an assured message transfer
	/// </summary>
	public class Transport : IClusterMessage
	{
		/// <summary>
		/// Message ID
		/// </summary>
		public Guid MessageID
		{
			get;
			set;
		}

		/// <summary>
		/// Message
		/// </summary>
		public IClusterMessage Message
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
			Endpoint.AssuredTransport(this.MessageID, this.Message);
			return Task.FromResult<bool>(true);
		}
	}
}
