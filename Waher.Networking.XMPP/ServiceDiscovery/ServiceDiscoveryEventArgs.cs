using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Delegate for service discovery events or callback methods.
	/// </summary>
	/// <param name="Client"></param>
	/// <param name="e"></param>
	public delegate void ServiceDiscoveryEventHandler(XmppClient Client, ServiceDiscoveryEventArgs e);

	/// <summary>
	/// Event arguments for service discovery responses.
	/// </summary>
	public class ServiceDiscoveryEventArgs : IqResultEventArgs
	{
		private Dictionary<string, bool> features;
		private Identity[] identities;

		internal ServiceDiscoveryEventArgs(IqResultEventArgs e, Dictionary<string, bool> Features, Identity[] Identities)
			: base(e)
		{
			this.features = Features;
			this.identities = Identities;
		}

		/// <summary>
		/// Features
		/// </summary>
		public Dictionary<string, bool> Features { get { return this.features; } }

		/// <summary>
		/// Identities
		/// </summary>
		public Identity[] Identities { get { return this.identities; } }
	}
}
