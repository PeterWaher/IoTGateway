using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Delegate for clock source callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event argument</param>
	public delegate void ClockSourceEventHandler(object Sender, ClockSourceEventArgs e);

	/// <summary>
	/// Event arguments containing the response of a clock source request.
	/// </summary>
	public class ClockSourceEventArgs : IqResultEventArgs
    {
		private readonly string clockSourceJID;
		
		internal ClockSourceEventArgs(string Jid, IqResultEventArgs e)
			: base(e)
		{
			this.clockSourceJID = Jid;
		}

		/// <summary>
		/// JID of clock source.
		/// </summary>
		public string ClockSourceJID => this.clockSourceJID;
	}
}
