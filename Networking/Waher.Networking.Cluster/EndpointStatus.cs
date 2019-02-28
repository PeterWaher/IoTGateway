using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Contains information about one of the endpoints in the cluster.
	/// </summary>
	public class EndpointStatus
	{
		private IPEndPoint endpoint;
		private object status;

		internal EndpointStatus(IPEndPoint Endpoint, object Status)
		{
			this.endpoint = Endpoint;
			this.status = Status;
		}

		/// <summary>
		/// Remote endpoint
		/// </summary>
		public IPEndPoint Endpoint => this.endpoint;

		/// <summary>
		/// Endpoint status.
		/// </summary>
		public object Status => this.status;
	}
}
