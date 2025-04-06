using System.Net.Mail;
using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for legal identity petitions
	/// </summary>
	public class LegalIdentityPetitionEventArgs : PetitionEventArgs
	{
		private readonly string requestedIdentityId;

		/// <summary>
		/// Event arguments for legal identity petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestorIdentity">Legal Identity of entity making the request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="RequestedIdentityId">Petition for this legal identity.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Purpose">Purpose of petitioning the identity information.</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		/// <param name="Context">Any machine-readable context XML element available in the petition.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public LegalIdentityPetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string RequestedIdentityId, string PetitionId, string Purpose, string ClientEndpoint, XmlElement Context,
			string[] Properties, string[] Attachments)
			: base(e, RequestorIdentity, RequestorFullJid, PetitionId, Purpose, ClientEndpoint, Context, Properties, Attachments)
		{
			this.requestedIdentityId = RequestedIdentityId;
		}

		/// <summary>
		/// Requested identity ID
		/// </summary>
		public string RequestedIdentityId => this.requestedIdentityId;
	}
}
