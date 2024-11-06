using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
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
