namespace Waher.Security.DTLS
{
	/// <summary>
	/// Event arguments for handshake failure events.
	/// </summary>
	public class FailureEventArgs : RemoteEndpointEventArgs
	{
		private readonly string reason;
		private readonly AlertDescription descripton;

		/// <summary>
		/// Event arguments for handshake failure events.
		/// </summary>
		/// <param name="State">Remote endpoint state object.</param>
		/// <param name="Reason">Reason for failing.</param>
		/// <param name="Descripton">Alert description.</param>
		public FailureEventArgs(EndpointState State, string Reason, AlertDescription Descripton)
			: base(State)
		{
			this.reason = Reason;
			this.descripton = Descripton;
		}

		/// <summary>
		/// Reason for failing.
		/// </summary>
		public string Reason => this.reason;

		/// <summary>
		/// Alert description.
		/// </summary>
		public AlertDescription Description => this.descripton;
	}
}
