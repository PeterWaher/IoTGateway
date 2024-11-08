namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// SOCKS5 command.
	/// </summary>
	public enum Command
	{
		/// <summary>
		/// CONNECT command (1)
		/// </summary>
		CONNECT = 1,

		/// <summary>
		/// BIND command (2)
		/// </summary>
		BIND = 2,

		/// <summary>
		/// UDP_ASSOCIATE command (3)
		/// </summary>
		UDP_ASSOCIATE = 3
	}
}
