using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for legal identities callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task LegalIdentitiesEventHandler(object Sender, LegalIdentitiesEventArgs e);

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
