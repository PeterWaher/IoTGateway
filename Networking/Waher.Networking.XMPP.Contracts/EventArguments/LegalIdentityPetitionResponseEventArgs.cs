﻿using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for legal identity petition response events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task LegalIdentityPetitionResponseEventHandler(object Sender, LegalIdentityPetitionResponseEventArgs e);

	/// <summary>
	/// Event arguments for legal identity petition responses
	/// </summary>
	public class LegalIdentityPetitionResponseEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestedIdentity;
		private readonly string petitionId;
		private readonly bool response;
		private readonly string clientEndpoint;

		/// <summary>
		/// Event arguments for legal identity petition responses
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestedIdentity">Requested identity, if accepted, null if rejected.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Response">If accepted (true) or rejected (false).</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		public LegalIdentityPetitionResponseEventArgs(MessageEventArgs e, LegalIdentity RequestedIdentity, string PetitionId, 
			bool Response, string ClientEndpoint)
			: base(e)
		{
			this.requestedIdentity = RequestedIdentity;
			this.petitionId = PetitionId;
			this.response = Response;
			this.clientEndpoint = ClientEndpoint;
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
		/// If accepted (true) or rejected (false).
		/// </summary>
		public bool Response => this.response;

		/// <summary>
		/// Remote endpoint of remote party client.
		/// </summary>
		public string ClientEndpoint => this.clientEndpoint;
	}
}
