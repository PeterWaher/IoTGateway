using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
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
		private readonly string clientEndpoint;
		private readonly XmlElement context;

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
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		/// <param name="Context">Any machine-readable context XML element available in the petition.</param>
		public SignaturePetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string SignatoryIdentityId, string PetitionId, string Purpose, byte[] Content, string ClientEndpoint, XmlElement Context)
			: base(e)
		{
			this.requestorIdentity = RequestorIdentity;
			this.requestorFullJid = RequestorFullJid;
			this.signatoryIdentityId = SignatoryIdentityId;
			this.petitionId = PetitionId;
			this.purpose = Purpose;
			this.content = Content;
			this.clientEndpoint = ClientEndpoint;
			this.context = Context;
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

		/// <summary>
		/// Remote endpoint of remote party client.
		/// </summary>
		public string ClientEndpoint => this.clientEndpoint;

		/// <summary>
		/// Any machine-readable context XML element available in the petition.
		/// </summary>
		public XmlElement Context => this.context;
	}
}
