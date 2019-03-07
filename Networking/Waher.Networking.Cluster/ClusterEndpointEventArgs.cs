using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster endpoint events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterEndpointEventHandler(object Sender, ClusterEndpointEventArgs e);

	/// <summary>
	/// Event arguments for cluster endpoint events.
	/// </summary>
	public class ClusterEndpointEventArgs : EventArgs
	{
		private readonly IPEndPoint endpoint;

		/// <summary>
		/// Event arguments for cluster endpoint events.
		/// </summary>
		/// <param name="Endpoint">Identity of cluster endpoiunt.</param>
		public ClusterEndpointEventArgs(IPEndPoint Endpoint)
			: base()
		{
			this.endpoint = Endpoint;
		}

		/// <summary>
		/// Identity of cluster endpoiunt.
		/// </summary>
		public IPEndPoint Endpoint => this.endpoint;
	}
}
