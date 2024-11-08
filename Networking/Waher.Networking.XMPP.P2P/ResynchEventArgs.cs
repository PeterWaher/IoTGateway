using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Peer connection event arguments.
	/// </summary>
	public class ResynchEventArgs : EventArgs
	{
		private readonly EventHandlerAsync<ResynchEventArgs> callback;
		private readonly string remoteFullJid;
		private bool ok = false;

		/// <summary>
		/// Peer connection event arguments.
		/// </summary>
		/// <param name="RemoteFullJid">Remote Full JID.</param>
		/// <param name="Callback">Callback method.</param>
		public ResynchEventArgs(string RemoteFullJid, EventHandlerAsync<ResynchEventArgs> Callback)
		{
			this.remoteFullJid = RemoteFullJid;
			this.callback = Callback;
		}

		/// <summary>
		/// JID of the remote end-point.
		/// </summary>
		public string RemoteFullJid => this.remoteFullJid;

		/// <summary>
		/// If the synchronization method succeeded (true) or failed (false).
		/// </summary>
		public bool Ok => this.ok;

		/// <summary>
		/// Method called by callback, to report that the synchronization succeeded (<paramref name="Ok"/>=true), or
		/// failed (<paramref name="Ok"/>=false).
		/// </summary>
		/// <param name="Ok">If the synchronization method succeeded (true) or failed (false).</param>
		public Task Done(bool Ok)
		{
			this.ok = Ok;
			return this.callback.Raise(this, this);
		}
	}
}
