﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Messages
{
	/// <summary>
	/// Ping message that can be sent to test endpoints in cluster receive messages.
	/// </summary>
	public class Ping : IClusterMessage
	{
		/// <summary>
		/// Method called when the message has been received.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>If the message was accepted/processed or not.
		/// In Acknowledged service, this corresponds to ACK/NACK.</returns>
		public Task<bool> MessageReceived(ClusterEndpoint Endpoint, IPEndPoint RemoteEndpoint)
		{
			return Task.FromResult(true);
		}
	}
}
