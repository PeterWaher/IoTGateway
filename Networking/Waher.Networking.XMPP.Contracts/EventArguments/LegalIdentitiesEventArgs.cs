using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for legal identities responses
	/// </summary>
	public class LegalIdentitiesEventArgs : IqResultEventArgs
	{
		private readonly LegalIdentity[] identities;

		/// <summary>
		/// Event arguments for legal identities responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Identities">Legal Identities.</param>
		public LegalIdentitiesEventArgs(IqResultEventArgs e, LegalIdentity[] Identities)
			: base(e)
		{
			this.identities = Identities;
		}

		/// <summary>
		/// Legal Identities
		/// </summary>
		public LegalIdentity[] Identities => this.identities;
	}
}
