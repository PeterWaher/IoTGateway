using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster message acknowledgement events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterMessageAckEventHandler(object Sender, ClusterMessageAckEventArgs e);

	/// <summary>
	/// Event arguments for cluster message acknowledgement events.
	/// </summary>
	public class ClusterMessageAckEventArgs : ClusterMessageEventArgs 
	{
		private readonly EndpointAcknowledgement[] responses;
		private readonly object state;

		/// <summary>
		/// Event arguments for cluster message acknowledgement events.
		/// </summary>
		/// <param name="Message">Message object</param>
		/// <param name="Responses">Message acknowledgement responses</param>
		/// <param name="State">State object passed on to the original request.</param>
		public ClusterMessageAckEventArgs(IClusterMessage Message, EndpointAcknowledgement[] Responses, object State)
			: base(Message)
		{
			this.responses = Responses;
			this.state = State;
		}

		/// <summary>
		/// Message acknowledgement responses.
		/// </summary>
		public EndpointAcknowledgement[] Responses => this.responses;

		/// <summary>
		/// State object passed on to the original request.
		/// </summary>
		public object State => this.state;
	}
}
