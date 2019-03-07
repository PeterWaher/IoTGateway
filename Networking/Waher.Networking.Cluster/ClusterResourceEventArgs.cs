using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster resource events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterResourceEventHandler(object Sender, ClusterResourceEventArgs e);

	/// <summary>
	/// Event arguments for cluster resource events.
	/// </summary>
	public class ClusterResourceEventArgs : EventArgs
	{
		private readonly string resource;
		private readonly object state;

		/// <summary>
		/// Event arguments for cluster resource events.
		/// </summary>
		/// <param name="Resource">Resource name</param>
		/// <param name="State">State object passed on to the original request.</param>
		public ClusterResourceEventArgs(string Resource, object State)
		{
			this.resource = Resource;
			this.state = State;
		}

		/// <summary>
		/// Resource name
		/// </summary>
		public string Resource => this.resource;

		/// <summary>
		/// State object passed on to the original request.
		/// </summary>
		public object State => this.state;
	}
}
