using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Resynchronization event handler delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ResynchEventHandler(object Sender, ResynchEventArgs e);

	/// <summary>
	/// Peer connection event arguments.
	/// </summary>
	public class ResynchEventArgs : EventArgs
	{
		private readonly ResynchEventHandler callback;
		private readonly string remoteFullJid;
		private bool ok = false;

		/// <summary>
		/// Peer connection event arguments.
		/// </summary>
		/// <param name="RemoteFullJid">Remote Full JID.</param>
		/// <param name="Callback">Callback method.</param>
		public ResynchEventArgs(string RemoteFullJid, ResynchEventHandler Callback)
		{
			this.remoteFullJid = RemoteFullJid;
			this.callback = Callback;
		}

		/// <summary>
		/// JID of the remote end-point.
		/// </summary>
		public string RemoteFullJid
		{
			get { return this.remoteFullJid; }
		}

		/// <summary>
		/// If the synchronization method succeeded (true) or failed (false).
		/// </summary>
		public bool Ok
		{
			get { return this.ok; }
		}

		/// <summary>
		/// Method called by callback, to report that the synchronization succeeded (<paramref name="Ok"/>=true), or
		/// failed (<paramref name="Ok"/>=false).
		/// </summary>
		/// <param name="Ok">If the synchronization method succeeded (true) or failed (false).</param>
		public void Done(bool Ok)
		{
			this.ok = Ok;

			if (this.callback != null)
			{
				try
				{
					this.callback(this, this);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}
	}
}
