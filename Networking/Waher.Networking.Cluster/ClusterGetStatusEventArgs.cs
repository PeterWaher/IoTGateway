using System;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster get status events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate Task ClusterGetStatusEventHandler(object Sender, ClusterGetStatusEventArgs e);

	/// <summary>
	/// Event arguments for cluster get status events.
	/// </summary>
	public class ClusterGetStatusEventArgs : EventArgs
	{
		private object status = null;

		/// <summary>
		/// Event arguments for cluster get status events.
		/// </summary>
		public ClusterGetStatusEventArgs()
		{
		}

		/// <summary>
		/// Current status of service.
		/// </summary>
		public object Status
		{
			get => this.status;
			set => this.status = value;
		}
	}
}
