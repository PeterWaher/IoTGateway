namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// State of Peer-to-peer network.
	/// </summary>
	public enum PeerToPeerNetworkState
	{
		/// <summary>
		/// Object created
		/// </summary>
		Created,

		/// <summary>
		/// Reinitializing after a network change.
		/// </summary>
		Reinitializing,

		/// <summary>
		/// Searching for Internet gateway.
		/// </summary>
		SearchingForGateway,

		/// <summary>
		/// Registering application in gateway.
		/// </summary>
		RegisteringApplicationInGateway,

		/// <summary>
		/// Ready to receive connections.
		/// </summary>
		Ready,

		/// <summary>
		/// Unable to create a peer-to-peer network that receives connections from the Internet.
		/// </summary>
		Error,

		/// <summary>
		/// Network is closed
		/// </summary>
		Closed
	}
}
