using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster endpoint status events.
	/// </summary>
	public class ClusterEndpointStatusEventArgs : ClusterEndpointEventArgs
	{
		private readonly object status;

		/// <summary>
		/// Event arguments for cluster endpoint status events.
		/// </summary>
		/// <param name="Endpoint">Identity of cluster endpoiunt.</param>
		/// <param name="Status">Endpoint status.</param>
		public ClusterEndpointStatusEventArgs(IPEndPoint Endpoint, object Status)
			: base(Endpoint)
		{
			this.status = Status;
		}

		/// <summary>
		/// Endpoint status.
		/// </summary>
		public object Status => this.status;
	}
}
