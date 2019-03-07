using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Message used to inform other members that the sending endpoint is releasing
	/// a locked resource.
	/// </summary>
	public class Release : IClusterMessage
	{
		/// <summary>
		/// Released resource.
		/// </summary>
		public string Resource
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
			Endpoint.Released(this.Resource);
			return Task.FromResult<bool>(true);
		}
	}
}
