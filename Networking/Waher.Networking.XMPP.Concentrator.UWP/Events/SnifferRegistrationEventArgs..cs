using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for sniffer registration callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SnifferRegistrationEventHandler(object Sender, SnifferRegistrationEventArgs e);

	/// <summary>
	/// Event arguments for sniffer registration responses.
	/// </summary>
	public class SnifferRegistrationEventArgs : IqResultEventArgs
	{
		private string snifferId;
		private DateTime expires;

		internal SnifferRegistrationEventArgs(string SnifferId, DateTime Expires, IqResultEventArgs Response)
			: base(Response)
		{
			this.snifferId = SnifferId;
			this.expires = Expires;
		}

		/// <summary>
		/// ID of sniffer session.
		/// </summary>
		public string SnifferrId
		{
			get { return this.snifferId; }
		}

		/// <summary>
		/// When the sniffer should expire, if not unregistered before.
		/// </summary>
		public DateTime Expires
		{
			get { return this.expires; }
		}
	}
}
