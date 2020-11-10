using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for signature petition response events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SignaturePetitionResponseEventHandler(object Sender, SignaturePetitionResponseEventArgs e);

	/// <summary>
	/// Event arguments for signature petition responses
	/// </summary>
	public class SignaturePetitionResponseEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestedIdentity;
		private readonly string petitionId;
		private readonly byte[] signature;
		private readonly bool response;

		/// <summary>
		/// Event arguments for signature petition responses
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestedIdentity">Requested identity, if accepted, null if rejected.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Signature">Digital Signature petitioned.</param>
		/// <param name="Response">If accepted (true) or rejected (false).</param>
		public SignaturePetitionResponseEventArgs(MessageEventArgs e, LegalIdentity RequestedIdentity, 
			string PetitionId, byte[] Signature, bool Response)
			: base(e)
		{
			this.requestedIdentity = RequestedIdentity;
			this.petitionId = PetitionId;
			this.signature = Signature;
			this.response = Response;
		}

		/// <summary>
		/// Requested identity, if accepted, null if rejected.
		/// </summary>
		public LegalIdentity RequestedIdentity => this.requestedIdentity;

		/// <summary>
		/// Petition ID
		/// </summary>
		public string PetitionId => this.petitionId;

		/// <summary>
		/// Digital Signature petitioned, if accepted by recipient.
		/// </summary>
		public byte[] Signature => this.signature;

		/// <summary>
		/// If accepted (true) or rejected (false).
		/// </summary>
		public bool Response => this.response;
	}
}
