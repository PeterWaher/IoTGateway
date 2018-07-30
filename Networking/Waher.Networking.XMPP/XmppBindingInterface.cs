using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Inteface to internal properties of the <see cref="XmppClient"/>, for use by instantiated alternative transports.
	/// </summary>
    public class XmppBindingInterface
    {
		private readonly XmppClient client;

		internal XmppBindingInterface(XmppClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// Stream header string.
		/// </summary>
		public string StreamHeader
		{
			get { return this.client.StreamHeader; }
			set { this.client.StreamHeader = value; }
		}

		/// <summary>
		/// Stream Footer string.
		/// </summary>
		public string StreamFooter
		{
			get { return this.client.StreamFooter; }
			set { this.client.StreamFooter = value; }
		}

		/// <summary>
		/// When next ping is due.
		/// </summary>
		public DateTime NextPing
		{
			get { return this.client.NextPing; }
			set { this.client.NextPing = value; }
		}

		/// <summary>
		/// Report an error on the connection.
		/// </summary>
		/// <param name="ex">Exception causing the error.</param>
		public void ConnectionError(Exception ex)
		{
			this.client.ConnectionError(ex);
		}

	}
}
