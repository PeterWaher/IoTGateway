using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// State of XMPP connection.
	/// </summary>
	public enum XmppState
	{
		/// <summary>
		/// Offline
		/// </summary>
		Offline,

		/// <summary>
		/// Connecting to broker.
		/// </summary>
		Connecting,

		/// <summary>
		/// Switching to encrypted channel
		/// </summary>
		StartingEncryption,

		/// <summary>
		/// Performing initial handshake.
		/// </summary>
		Authenticating,

		/// <summary>
		/// Connected.
		/// </summary>
		Connected,

		/// <summary>
		/// In an error state.
		/// </summary>
		Error
	}
}
