using System;

namespace Waher.Networking.XMPP.RDP
{
	/// <summary>
	/// State of a Remote Desktop Session
	/// </summary>
	public enum RemoteDesktopSessionState
	{
		/// <summary>
		/// Session is starting.
		/// </summary>
		Starting,

		/// <summary>
		/// Session has been started.
		/// </summary>
		Started,

		/// <summary>
		/// Session is stopping.
		/// </summary>
		Stopping,

		/// <summary>
		/// Session has been stopped.
		/// </summary>
		Stopped
	}
}
