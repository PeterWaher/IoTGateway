using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster endpoint status events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate Task ClusterEndpointStatusEventHandler(object Sender, ClusterEndpointStatusEventArgs e);

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
