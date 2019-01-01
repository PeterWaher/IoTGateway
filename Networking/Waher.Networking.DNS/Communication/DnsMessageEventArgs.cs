using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Delegate for DNS Message event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void DnsMessageEventHandler(object Sender, DnsMessageEventArgs e);

	/// <summary>
	/// DNS Messge event arguments.
	/// </summary>
	public class DnsMessageEventArgs : EventArgs
	{
		private readonly DnsMessage message;
		private readonly object state;

		/// <summary>
		/// DNS Messge event arguments.
		/// </summary>
		/// <param name="Message">DNS Message</param>
		/// <param name="State">State object</param>
		public DnsMessageEventArgs(DnsMessage Message, object State)
		{
			this.message = Message;
			this.state = State;
		}

		/// <summary>
		/// DNS Message
		/// </summary>
		public DnsMessage Message => this.message;

		/// <summary>
		/// State object
		/// </summary>
		public object State => this.state;
	}
}
