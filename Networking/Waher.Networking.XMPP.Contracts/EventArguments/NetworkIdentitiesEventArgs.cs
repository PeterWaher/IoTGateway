using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for network identities callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void NetworkIdentitiesEventHandler(object Sender, NetworkIdentitiesEventArgs e);

	/// <summary>
	/// Event arguments for network identities responses
	/// </summary>
	public class NetworkIdentitiesEventArgs : IqResultEventArgs
	{
		private readonly NetworkIdentity[] identities;

		/// <summary>
		/// Event arguments for network identities responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Identities">Network Identities.</param>
		public NetworkIdentitiesEventArgs(IqResultEventArgs e, NetworkIdentity[] Identities)
			: base(e)
		{
			this.identities = Identities;
		}

		/// <summary>
		/// Network Identities
		/// </summary>
		public NetworkIdentity[] Identities => this.identities;
	}
}
