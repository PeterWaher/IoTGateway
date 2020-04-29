using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms;

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
		private readonly Dictionary<string, DataForm> extendedInformation;
		private readonly Dictionary<string, bool> features;
		private readonly Identity[] identities;

		internal ServiceDiscoveryEventArgs(IqResultEventArgs e, Identity[] Identities, 
			Dictionary<string, bool> Features, Dictionary<string, DataForm> ExtendedInformation)
			: base(e)
		{
			this.identities = Identities;
			this.features = Features;
			this.extendedInformation = ExtendedInformation;
		}

		/// <summary>
		/// Identities
		/// </summary>
		public Identity[] Identities
		{
			get { return this.identities; }
		}

		/// <summary>
		/// Features
		/// </summary>
		public Dictionary<string, bool> Features
		{
			get { return this.features; }
		}

		/// <summary>
		/// Extended information, as defined in XEP-0128: Service Discovery Extensions: http://xmpp.org/extensions/xep-0128.html
		/// </summary>
		public Dictionary<string, DataForm> ExtendedInformation
		{
			get { return this.extendedInformation; }
		}

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
