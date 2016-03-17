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
		/// Stream Negotiation.
		/// </summary>
		StreamNegotiation,

		/// <summary>
		/// Switching to encrypted channel
		/// </summary>
		StartingEncryption,

		/// <summary>
		/// Performing user authentication.
		/// </summary>
		Authenticating,

		/// <summary>
		/// Performing session binding.
		/// </summary>
		Binding,

		/// <summary>
		/// Requesting session from server.
		/// </summary>
		RequestingSession,

		/// <summary>
		/// Fetching roster.
		/// </summary>
		FetchingRoster,

		/// <summary>
		/// Setting presence.
		/// </summary>
		SettingPresence,

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
