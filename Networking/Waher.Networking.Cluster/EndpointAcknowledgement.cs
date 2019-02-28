using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Contains information about one of the endpoints in the cluster.
	/// </summary>
	public class EndpointAcknowledgement
	{
		private readonly IPEndPoint endpoint;
		private readonly bool? ack;

		internal EndpointAcknowledgement(IPEndPoint Endpoint, bool? Ack)
		{
			this.endpoint = Endpoint;
			this.ack = Ack;
		}

		/// <summary>
		/// Remote endpoint
		/// </summary>
		public IPEndPoint Endpoint => this.endpoint;

		/// <summary>
		/// Acknowledgement response:
		/// 
		/// true = Message actively acknowledged (ACK)
		/// false = Message actively rejected, or not acknowledged (NACK)
		/// null = Message not acknowledged or rejected.
		/// </summary>
		public bool? ACK => this.ack;
	}
}
