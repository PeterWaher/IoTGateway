using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Delegate for state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void StateChangedEventHandler(object Sender, StateChangedEventArgs e);

	/// <summary>
	/// Event arguments for state change events.
	/// </summary>
	public class StateChangedEventArgs : EventArgs
	{
		private object remoteEndpoint;
		private DtlsState state;

		/// <summary>
		/// Event arguments for state change events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public StateChangedEventArgs(object RemoteEndpoint, DtlsState State)
		{
			this.remoteEndpoint = RemoteEndpoint;
			this.state = State;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public object RemoteEndpoint
		{
			get { return this.remoteEndpoint; }
		}

		/// <summary>
		/// Endpoint state.
		/// </summary>
		public DtlsState State
		{
			get { return this.state; }
		}
	}
}
