using System;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Base class for event arguments with a remote endpoint attribute.
	/// </summary>
	public class RemoteEndpointEventArgs : EventArgs
	{
		internal readonly EndpointState state;

		/// <summary>
		/// Event arguments for state change events.
		/// </summary>
		/// <param name="State">Remote endpoint state object.</param>
		public RemoteEndpointEventArgs(EndpointState State)
		{
			this.state = State;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public object RemoteEndpoint => this.state.remoteEndpoint;

		/// <summary>
		/// UTF-8 encoded authenticated Identity, if available, null otherwise.
		/// </summary>
		public byte[] Identity => this.state.credentials?.Identity;

		/// <summary>
		/// Authenticated Identity string, if available, null otherwise.
		/// </summary>
		public string IdentityString => this.state.credentials?.IdentityString;
	}
}
