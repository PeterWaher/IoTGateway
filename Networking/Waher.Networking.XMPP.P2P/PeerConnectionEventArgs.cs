using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Peer connection event arguments.
	/// </summary>
	public class PeerConnectionEventArgs
	{
		private XmppClient client;
		private object state;

		/// <summary>
		/// Peer connection event arguments.
		/// </summary>
		/// <param name="Client">XMPP client, if aquired, or null otherwise.</param>
		/// <param name="State">State object passed to the original request.</param>
		public PeerConnectionEventArgs(XmppClient Client, object State)
		{
			this.client = Client;
			this.state = State;
		}

		/// <summary>
		/// XMPP client, if aquired, or null otherwise.
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}
	}
}
