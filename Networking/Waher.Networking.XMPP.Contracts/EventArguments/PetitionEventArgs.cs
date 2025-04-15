using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Abstract base class for petition event arguments.
	/// </summary>
	public abstract class PetitionEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestorIdentity;
		private readonly string requestorFullJid;
		private readonly string petitionId;
		private readonly string purpose;
		private readonly string clientEndpoint;
		private readonly XmlElement context;
		private readonly string[] properties;
		private readonly string[] attachments;

		/// <summary>
		/// Abstract base class for petition event arguments.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestorIdentity">Legal Identity of entity making the request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Purpose">Purpose of petitioning the identity information.</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		/// <param name="Context">Any machine-readable context XML element available in the petition.</param>
		/// <param name="Properties">Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</param>
		/// <param name="Attachments">Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.</param>
		public PetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string PetitionId, string Purpose, string ClientEndpoint, XmlElement Context, 
			string[] Properties, string[] Attachments)
			: base(e)
		{
			this.requestorIdentity = RequestorIdentity;
			this.requestorFullJid = RequestorFullJid;
			this.petitionId = PetitionId;
			this.purpose = Purpose;
			this.clientEndpoint = ClientEndpoint;
			this.context = Context;
			this.properties = Properties;
			this.attachments = Attachments;
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
		/// Petition ID
		/// </summary>
		public string PetitionId => this.petitionId;

		/// <summary>
		/// Purpose
		/// </summary>
		public string Purpose => this.purpose;

		/// <summary>
		/// Remote endpoint of remote party client.
		/// </summary>
		public string ClientEndpoint => this.clientEndpoint;

		/// <summary>
		/// Any machine-readable context XML element available in the petition.
		/// </summary>
		public XmlElement Context => this.context;

		/// <summary>Optional property hints to provide to the remote party,
		/// highlighting which properties will be used in the response.</summary>
		public string[] Properties => this.properties;

		/// <summary>
		/// Optional attachment hints to provide to the remote party,
		/// highlighting which attachments will be used in the response.
		/// </summary>
		public string[] Attachments => this.attachments;
	}
}
