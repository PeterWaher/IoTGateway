namespace Waher.Security.DTLS
{
	/// <summary>
	/// Event arguments for state change events.
	/// </summary>
	public class StateChangedEventArgs : RemoteEndpointEventArgs
	{
		private readonly DtlsState newState;

		/// <summary>
		/// Event arguments for state change events.
		/// </summary>
		/// <param name="State">Remote endpoint state object.</param>
		/// <param name="NewState">New Endpoint state.</param>
		public StateChangedEventArgs(EndpointState State, DtlsState NewState)
			: base(State)
		{
			this.newState = NewState;
		}

		/// <summary>
		/// Endpoint state.
		/// </summary>
		public DtlsState State => this.newState;
	}
}
