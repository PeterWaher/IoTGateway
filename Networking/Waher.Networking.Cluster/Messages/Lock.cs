using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Message used to inform other members that the sending endpoint is trying
	/// to lock a resource. Any recipient already having the resource locked NACKs 
	/// the message.
	/// </summary>
	public class Lock : IClusterMessage
	{
		/// <summary>
		/// Resource to lock.
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
			lock (Endpoint.lockedResources)
			{
				return Task.FromResult<bool>(!Endpoint.lockedResources.ContainsKey(this.Resource));
			}
		}
	}
}
