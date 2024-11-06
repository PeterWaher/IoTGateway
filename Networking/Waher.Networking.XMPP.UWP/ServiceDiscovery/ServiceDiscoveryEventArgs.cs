using System.Collections.Generic;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Event arguments for service discovery responses.
	/// </summary>
	public class ServiceDiscoveryEventArgs : IqResultEventArgs
	{
		private readonly Dictionary<string, DataForm> extendedInformation;
		private readonly Dictionary<string, bool> features;
		private readonly Identity[] identities;

		/// <summary>
		/// Event arguments for service discovery responses.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected ServiceDiscoveryEventArgs(ServiceDiscoveryEventArgs e)
			: base(e)
		{
			this.identities = e.identities;
			this.features = e.features;
			this.extendedInformation = e.extendedInformation;
		}

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
		public Identity[] Identities => this.identities;

		/// <summary>
		/// Features
		/// </summary>
		public Dictionary<string, bool> Features => this.features;

		/// <summary>
		/// Extended information, as defined in XEP-0128: Service Discovery Extensions: http://xmpp.org/extensions/xep-0128.html
		/// </summary>
		public Dictionary<string, DataForm> ExtendedInformation => this.extendedInformation;

		/// <summary>
		/// Checks if the remote entity supports a specific feature.
		/// </summary>
		/// <param name="Feature">Name of feature.</param>
		/// <returns>If the feature is supported.</returns>
		public bool HasFeature(string Feature)
		{
			return this.features.ContainsKey(Feature);
		}

		/// <summary>
		/// Checks if the remote entity supports all of a set of features.
		/// </summary>
		/// <param name="Features">Set of features.</param>
		/// <returns>If all features are supported.</returns>
		public bool HasAllFeatures(params string[] Features)
		{
			foreach (string Feature in Features)
			{
				if (!this.HasFeature(Feature)) 
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the remote entity supports any of a set of features.
		/// </summary>
		/// <param name="Features">Set of features.</param>
		/// <returns>If any feature is supported.</returns>
		public bool HasAnyFeature(params string[] Features)
		{
			foreach (string Feature in Features)
			{
				if (this.HasFeature(Feature))
					return true;
			}

			return false;
		}
	}
}
