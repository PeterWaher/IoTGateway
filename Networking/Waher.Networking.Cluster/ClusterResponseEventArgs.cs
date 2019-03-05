using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster command response events.
	/// </summary>
	/// <typeparam name="ResponseType">Type of expected response.</typeparam>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterResponseEventHandler<ResponseType>(object Sender, ClusterResponseEventArgs<ResponseType> e);

	/// <summary>
	/// Event arguments for cluster message acknowledgement events.
	/// </summary>
	public class ClusterResponseEventArgs<ResponseType> : EventArgs 
	{
		private readonly IClusterCommand command;
		private readonly EndpointResponse<ResponseType>[] responses;
		private readonly object state;

		/// <summary>
		/// Event arguments for cluster message acknowledgement events.
		/// </summary>
		/// <param name="Command">Command object.</param>
		/// <param name="Responses">Message acknowledgement responses</param>
		/// <param name="State">State object passed on to the original request.</param>
		public ClusterResponseEventArgs(IClusterCommand Command, EndpointResponse<ResponseType>[] Responses, object State)
			: base()
		{
			this.command = Command;
			this.responses = Responses;
			this.state = State;
		}

		/// <summary>
		/// Cluster command object.
		/// </summary>
		public IClusterCommand Command => this.command;

		/// <summary>
		/// Command execution responses.
		/// </summary>
		public EndpointResponse<ResponseType>[] Responses => this.responses;

		/// <summary>
		/// State object passed on to the original request.
		/// </summary>
		public object State => this.state;

		/// <summary>
		/// If the command was executed without error on all endpoints.
		/// </summary>
		public bool Ok
		{
			get
			{
				foreach (EndpointResponse<ResponseType> P in this.responses)
				{
					if (!P.Ok)
						return false;
				}

				return true;
			}
		}
	}
}
