﻿using System;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Peer connection event arguments.
	/// </summary>
	public class PeerConnectionEventArgs : EventArgs
	{
		private readonly XmppClient client;
		private readonly object state;
		private readonly string localJid;
		private readonly string remoteJid;

		/// <summary>
		/// Peer connection event arguments.
		/// </summary>
		/// <param name="Client">XMPP client, if aquired, or null otherwise.</param>
		/// <param name="State">State object passed to the original request.</param>
		/// <param name="LocalJid">JID of the local end-point.</param>
		/// <param name="RemoteJid">JID of the remote end-point.</param>
		public PeerConnectionEventArgs(XmppClient Client, object State, string LocalJid, string RemoteJid)
		{
			this.client = Client;
			this.state = State;
			this.localJid = LocalJid;
			this.remoteJid = RemoteJid;
		}

		/// <summary>
		/// XMPP client, if aquired, or null otherwise.
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State => this.state;

		/// <summary>
		/// JID of the local end-point.
		/// </summary>
		public string LocalJid => this.localJid;

		/// <summary>
		/// JID of the remote end-point.
		/// </summary>
		public string RemoteJid => this.remoteJid;
	}
}
