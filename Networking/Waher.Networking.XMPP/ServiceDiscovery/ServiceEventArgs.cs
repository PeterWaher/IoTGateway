using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Delegate for service events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ServiceEventHandler(object Sender, ServiceEventArgs e);

	/// <summary>
	/// Event arguments for service responses.
	/// </summary>
	public class ServiceEventArgs : EventArgs
	{
		private readonly string jid;
		private readonly object state;

		/// <summary>
		/// Event arguments for service discovery responses.
		/// </summary>
		/// <param name="JID">Service JID.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public ServiceEventArgs(string JID, object State)
			: base()
		{
			this.jid = JID;
			this.state = State;
		}

		/// <summary>
		/// Service JID
		/// </summary>
		public string JID
		{
			get { return this.jid; }
		}

		/// <summary>
		/// State object to pass on to callback method.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}
	}
}
