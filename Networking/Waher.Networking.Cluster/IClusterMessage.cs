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
		/// <returns>If the message was accepted/processed or not.
		/// In Acknowledged service, this corresponds to ACK/NACK.</returns>
		bool MessageReceived();
	}
}
