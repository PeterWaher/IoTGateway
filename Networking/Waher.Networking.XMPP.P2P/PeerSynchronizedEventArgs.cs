using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Delegate for peer synchronization events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void PeerSynchronizedEventHandler(object Sender, PeerSynchronizedEventArgs e);

	/// <summary>
	/// Event arguments for peer synchronization events.
	/// </summary>
	public class PeerSynchronizedEventArgs : EventArgs
	{
		private readonly string fullJID;
		private readonly bool hasE2E;
		private readonly bool hasP2P;

		/// <summary>
		/// Event arguments for peer synchronization events.
		/// </summary>
		/// <param name="FullJID">Full JID of peer.</param>
		/// <param name="HasE2E">If End-to-End encryption information was found in presence stanza.</param>
		/// <param name="HasP2P">If Peer-to-peer address information was found in presence stanza.</param>
		public PeerSynchronizedEventArgs(string FullJID, bool HasE2E, bool HasP2P)
		{
			this.fullJID = FullJID;
			this.hasE2E = HasE2E;
			this.hasP2P = HasP2P;
		}

		/// <summary>
		/// Full JID of peer.
		/// </summary>
		public string FullJID => this.fullJID;

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
