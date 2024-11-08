using System;

namespace Waher.Networking.Cluster
{
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
