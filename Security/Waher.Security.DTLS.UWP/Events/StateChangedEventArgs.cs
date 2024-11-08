using System.Threading.Tasks;

namespace Waher.Security.DTLS
{
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
		public DtlsState State => this.state;
	}
}
