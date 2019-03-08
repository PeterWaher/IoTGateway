using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Keeps track of an outgoing command.
	/// </summary>
	internal abstract class CommandStatusBase
	{
		public Guid Id;
		public IClusterCommand Command;
		public byte[] CommandBinary;
		public DateTime Timeout;
		public DateTime TimeLimit;
		public object State;

		/// <summary>
		/// If all responses have been returned.
		/// </summary>
		/// <param name="Statuses">Valid statuses.</param>
		/// <returns>If all responses have been returned.</returns>
		public abstract bool IsComplete(EndpointStatus[] Statuses);

		/// <summary>
		/// Adds a response from an endpoint.
		/// </summary>
		/// <param name="From">Endpoint providing response</param>
		/// <param name="Response">Response object</param>
		public abstract void AddResponse(IPEndPoint From, object Response);

		/// <summary>
		/// Raises the response event.
		/// </summary>
		/// <param name="CurrentStatus">Current status of endpoints in cluster.</param>
		public abstract void RaiseResponseEvent(EndpointStatus[] CurrentStatus);

		/// <summary>
		/// Adds an error from an endpoint.
		/// </summary>
		/// <param name="From">Endpoint providing response</param>
		/// <param name="Error">Exception object describing the error.</param>
		public abstract void AddError(IPEndPoint From, Exception Error);
	}
}
