using System;

namespace Waher.Networking.Cluster
{
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
