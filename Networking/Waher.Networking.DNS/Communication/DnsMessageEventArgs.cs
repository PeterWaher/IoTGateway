using System;

namespace Waher.Networking.DNS.Communication
{
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
