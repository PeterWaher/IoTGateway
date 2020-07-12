using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for legal identity petition events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task LegalIdentityPetitionEventHandler(object Sender, LegalIdentityPetitionEventArgs e);

	/// <summary>
	/// Event arguments for legal identity petitions
	/// </summary>
	public class LegalIdentityPetitionEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestorIdentity;
		private readonly string requestorBareJid;
		private readonly string requestedIdentityId;
		private readonly string petitionId;
		private readonly string purpose;

		/// <summary>
		/// Event arguments for legal identity petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestorIdentity">Legal Identity of entity making the request.</param>
		/// <param name="RequestorBareJid">Full JID of requestor.</param>
		/// <param name="RequestedIdentityId">Petition for this legal identity.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Purpose">Purpose of petitioning the identity information.</param>
		public LegalIdentityPetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorBareJid,
			string RequestedIdentityId, string PetitionId, string Purpose)
			: base(e)
		{
			this.requestorIdentity = RequestorIdentity;
			this.requestorBareJid = RequestorBareJid;
			this.requestedIdentityId = RequestedIdentityId;
			this.petitionId = PetitionId;
			this.purpose = Purpose;
		}

		/// <summary>
		/// Legal Identity of requesting entity.
		/// </summary>
		public LegalIdentity RequestorIdentity => this.requestorIdentity;

		/// <summary>
		/// Bare JID of requestor.
		/// </summary>
		public string RequestorBareJid => this.requestorBareJid;

		/// <summary>
		/// Requested identity ID
		/// </summary>
		public string RequestedIdentityId => this.requestedIdentityId;

		/// <summary>
		/// Petition ID
		/// </summary>
		public string PetitionId => this.petitionId;

		/// <summary>
		/// Purpose
		/// </summary>
		public string Purpose => this.purpose;
	}
}
