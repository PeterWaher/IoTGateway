using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for digital signature petition events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SignaturePetitionEventHandler(object Sender, SignaturePetitionEventArgs e);

	/// <summary>
	/// Event arguments for digital signature petitions
	/// </summary>
	public class SignaturePetitionEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestorIdentity;
		private readonly string requestorFullJid;
		private readonly string signatoryIdentityId;
		private readonly string petitionId;
		private readonly string purpose;
		private readonly byte[] content;

		/// <summary>
		/// Event arguments for legal identity petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestorIdentity">Legal Identity of entity making the request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="SignatoryIdentityId">Legal identity of petitioned signatory.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Purpose">Purpose of petitioning the identity information.</param>
		/// <param name="Content">Content to sign.</param>
		public SignaturePetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string SignatoryIdentityId, string PetitionId, string Purpose, byte[] Content)
			: base(e)
		{
			this.requestorIdentity = RequestorIdentity;
			this.requestorFullJid = RequestorFullJid;
			this.signatoryIdentityId = SignatoryIdentityId;
			this.petitionId = PetitionId;
			this.purpose = Purpose;
			this.content = Content;
		}

		/// <summary>
		/// Legal Identity of requesting entity.
		/// </summary>
		public LegalIdentity RequestorIdentity => this.requestorIdentity;

		/// <summary>
		/// Full JID of requestor.
		/// </summary>
		public string RequestorFullJid => this.requestorFullJid;

		/// <summary>
		/// Legal identity of petitioned signatory.
		/// </summary>
		public string SignatoryIdentityId => this.signatoryIdentityId;

		/// <summary>
		/// Petition ID
		/// </summary>
		public string PetitionId => this.petitionId;

		/// <summary>
		/// Purpose
		/// </summary>
		public string Purpose => this.purpose;

		/// <summary>
		/// Content to sign.
		/// </summary>
		public byte[] ContentToSign => this.content;
	}
}
