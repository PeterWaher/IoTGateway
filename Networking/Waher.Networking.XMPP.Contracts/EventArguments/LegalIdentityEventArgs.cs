using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for legal identity callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task LegalIdentityEventHandler(object Sender, LegalIdentityEventArgs e);

	/// <summary>
	/// Event arguments for legal identity responses
	/// </summary>
	public class LegalIdentityEventArgs : IqResultEventArgs
	{
		private readonly LegalIdentity identity;

		/// <summary>
		/// Event arguments for legal identity responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Identity">Legal Identity.</param>
		public LegalIdentityEventArgs(IqResultEventArgs e, LegalIdentity Identity)
			: base(e)
		{
			this.identity = Identity;
		}

		/// <summary>
		/// Legal Identity
		/// </summary>
		public LegalIdentity Identity => this.identity;
	}
}
