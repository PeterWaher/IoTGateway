using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Delegate for availability events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task AvailableEventHandler(object Sender, AvailableEventArgs e);

	/// <summary>
	/// Event arguments for Availability events.
	/// </summary>
	public class AvailableEventArgs : EventArgs
	{
		private readonly PresenceEventArgs presence;
		private readonly bool hasE2E;
		private readonly bool hasP2P;

		/// <summary>
		/// Event arguments for Availability events.
		/// </summary>
		/// <param name="e">Presence event arguments.</param>
		/// <param name="HasE2E">If End-to-End encryption information was found in presence stanza.</param>
		/// <param name="HasP2P">If Peer-to-peer address information was found in presence stanza.</param>
		public AvailableEventArgs(PresenceEventArgs e, bool HasE2E, bool HasP2P)
		{
			this.presence = e;
			this.hasE2E = HasE2E;
			this.hasP2P = HasP2P;
		}

		/// <summary>
		/// Presence event arguments.
		/// </summary>
		public PresenceEventArgs Presence => this.presence;

		/// <summary>
		/// If End-to-End encryption information was found in presence stanza.
		/// </summary>
		public bool HasE2E => this.hasE2E;

		/// <summary>
		/// If Peer-to-peer address information was found in presence stanza.
		/// </summary>
		public bool HasP2P => this.hasP2P;
	}
}
