using System;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for sniffer registration responses.
	/// </summary>
	public class SnifferRegistrationEventArgs : IqResultEventArgs
	{
		private readonly string snifferId;
		private readonly DateTime expires;

		internal SnifferRegistrationEventArgs(string SnifferId, DateTime Expires, IqResultEventArgs Response)
			: base(Response)
		{
			this.snifferId = SnifferId;
			this.expires = Expires;
		}

		/// <summary>
		/// ID of sniffer session.
		/// </summary>
		public string SnifferId => this.snifferId;

		/// <summary>
		/// When the sniffer should expire, if not unregistered before.
		/// </summary>
		public DateTime Expires => this.expires;
	}
}
