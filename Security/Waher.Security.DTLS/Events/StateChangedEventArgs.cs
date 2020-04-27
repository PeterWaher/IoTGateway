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
	public class StateChangedEventArgs : RemoteEndpointEventArgs
	{
		private readonly DtlsState state;

		/// <summary>
		/// Event arguments for state change events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public StateChangedEventArgs(object RemoteEndpoint, DtlsState State)
			: base(RemoteEndpoint)
		{
			this.state = State;
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
