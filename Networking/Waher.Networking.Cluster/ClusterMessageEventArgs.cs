using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster message events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterMessageEventHandler(object Sender, ClusterMessageEventArgs e);

	/// <summary>
	/// Event arguments for cluster message events.
	/// </summary>
	public class ClusterMessageEventArgs : EventArgs
	{
		private readonly IClusterMessage message;

		/// <summary>
		/// Event arguments for cluster message events.
		/// </summary>
		/// <param name="Message">Message object</param>
		public ClusterMessageEventArgs(IClusterMessage Message)
		{
			this.message = Message;
		}

		/// <summary>
		/// Message object
		/// </summary>
		public IClusterMessage Message => this.message;
	}
}
