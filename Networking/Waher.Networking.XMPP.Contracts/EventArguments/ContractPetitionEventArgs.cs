using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for smart contract petition events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractPetitionEventHandler(object Sender, ContractPetitionEventArgs e);

	/// <summary>
	/// Event arguments for smart contract petitions
	/// </summary>
	public class ContractPetitionEventArgs : MessageEventArgs
	{
		private readonly LegalIdentity requestorIdentity;
		private readonly string requestorFullJid;
		private readonly string requestedContractId;
		private readonly string petitionId;
		private readonly string purpose;
		private readonly string clientEndpoint;

		/// <summary>
		/// Event arguments for smart contract petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestorIdentity">Legal Identity of entity making the request.</param>
		/// <param name="RequestorFullJid">Full JID of requestor.</param>
		/// <param name="RequestedContractId">Petition for this smart contract.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Purpose">Purpose of petitioning the identity information.</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		public ContractPetitionEventArgs(MessageEventArgs e, LegalIdentity RequestorIdentity, string RequestorFullJid,
			string RequestedContractId, string PetitionId, string Purpose, string ClientEndpoint)
			: base(e)
		{
			this.requestorIdentity = RequestorIdentity;
			this.requestorFullJid = RequestorFullJid;
			this.requestedContractId = RequestedContractId;
			this.petitionId = PetitionId;
			this.purpose = Purpose;
			this.clientEndpoint = ClientEndpoint;
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
		/// Requested contract ID
		/// </summary>
		public string RequestedContractId => this.requestedContractId;

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
	}
}
