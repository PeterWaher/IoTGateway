namespace Waher.Networking.XMPP.Chat.ChatStates
{
	/// <summary>
	/// Negotiation state for chat state notifications.
	/// </summary>
	public enum ChatStateNegotiationState
	{
		/// <summary>
		/// Unknown support state.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Support requested.
		/// </summary>
		Requested = 1,

		/// <summary>
		/// Supported.
		/// </summary>
		Supported = 2,

		/// <summary>
		/// Not supported.
		/// </summary>
		NotSupported = 3
	}
}
