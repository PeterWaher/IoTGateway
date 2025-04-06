using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for digital signature petitions
	/// </summary>
	public class SignaturePetitionEventArgs : PetitionEventArgs
	{
		private readonly string signatoryIdentityId;
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
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		/// <param name="Context">Any machine-readable context XML element available in the petition.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public SignaturePetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string SignatoryIdentityId, string PetitionId, string Purpose, byte[] Content, string ClientEndpoint, XmlElement Context,
			string[] Properties, string[] Attachments)
			: base(e, RequestorIdentity, RequestorFullJid, PetitionId, Purpose, ClientEndpoint, Context, Properties, Attachments)
		{
			this.signatoryIdentityId = SignatoryIdentityId;
			this.content = Content;
		}

		/// <summary>
		/// Legal identity of petitioned signatory.
		/// </summary>
		public string SignatoryIdentityId => this.signatoryIdentityId;

		/// <summary>
		/// Content to sign.
		/// </summary>
		public byte[] ContentToSign => this.content;
	}
}
