using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for smart contract petition responses
	/// </summary>
	public class ContractPetitionResponseEventArgs : MessageEventArgs
	{
		private readonly Contract requestedContract;
		private readonly string petitionId;
		private readonly bool response;
		private readonly string clientEndpoint;
		private readonly XmlElement context;

		/// <summary>
		/// Event arguments for smart contract petition responses
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestedContract">Requested contract, if accepted, null if rejected.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Response">If accepted (true) or rejected (false).</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		/// <param name="Context">Any machine-readable context XML element available in the petition response.</param>
		public ContractPetitionResponseEventArgs(MessageEventArgs e, Contract RequestedContract, string PetitionId, 
			bool Response, string ClientEndpoint, XmlElement Context)
			: base(e)
		{
			this.requestedContract = RequestedContract;
			this.petitionId = PetitionId;
			this.response = Response;
			this.clientEndpoint = ClientEndpoint;
			this.context = Context;
		}

		/// <summary>
		/// Requested contract, if accepted, null if rejected.
		/// </summary>
		public Contract RequestedContract => this.requestedContract;

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

		/// <summary>
		/// Any machine-readable context XML element available in the petition.
		/// </summary>
		public XmlElement Context => this.context;
	}
}
