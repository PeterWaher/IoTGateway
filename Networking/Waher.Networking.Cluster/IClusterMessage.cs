using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Interface for cluster messages.
	/// </summary>
	public interface IClusterMessage : IClusterObject
	{
		/// <summary>
		/// Method called when the message has been received.
		/// </summary>
		void MessageReceived();
	}
}
