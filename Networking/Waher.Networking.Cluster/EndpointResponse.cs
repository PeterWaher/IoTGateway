using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Contains the response from one of the endpoints in the cluster.
	/// </summary>
	public class EndpointResponse<ResponseType>
	{
		private readonly IPEndPoint endpoint;
		private readonly ResponseType response;
		private readonly Exception error;

		internal EndpointResponse(IPEndPoint Endpoint, ResponseType Response,
			Exception Error)
		{
			this.endpoint = Endpoint;
			this.response = Response;
			this.error = Error;
		}

		/// <summary>
		/// Remote endpoint
		/// </summary>
		public IPEndPoint Endpoint => this.endpoint;

		/// <summary>
		/// Response from endpoint
		/// </summary>
		public ResponseType Response => this.response;

		/// <summary>
		/// Any error returned from the endpoint.
		/// </summary>
		public Exception Error => this.error;

		/// <summary>
		/// If the command was executed without error on the endpoint.
		/// </summary>
		public bool Ok => this.error is null;
	}
}
