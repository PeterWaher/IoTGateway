﻿using System;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Event arguments for service responses.
	/// </summary>
	public class ServiceEventArgs : EventArgs
	{
		private readonly string jid;
		private readonly string feature;
		private readonly object state;

		/// <summary>
		/// Event arguments for service discovery responses.
		/// </summary>
		/// <param name="JID">Service JID.</param>
		/// <param name="Feature">Feature requested.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public ServiceEventArgs(string JID, string Feature, object State)
			: base()
		{
			this.jid = JID;
			this.state = State;
			this.feature = Feature;
		}

		/// <summary>
		/// Service JID
		/// </summary>
		public string JID => this.jid;

		/// <summary>
		/// Service Feature
		/// </summary>
		public string Feature => this.feature;

		/// <summary>
		/// State object to pass on to callback method.
		/// </summary>
		public object State => this.state;
	}
}
