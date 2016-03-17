using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Delegate for service discovery events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ServiceDiscoveryEventHandler(object Sender, ServiceDiscoveryEventArgs e);

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

		/// <summary>
		/// Checks if the remote entity supports a specific feature.
		/// </summary>
		/// <param name="Feature">Name of feature.</param>
		/// <returns>If the feature is supported.</returns>
		public bool HasFeature(string Feature)
		{
			return this.features.ContainsKey(Feature);
		}
	}
}
