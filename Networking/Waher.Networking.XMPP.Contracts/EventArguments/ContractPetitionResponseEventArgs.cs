using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for smart contract petition response events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractPetitionResponseEventHandler(object Sender, ContractPetitionResponseEventArgs e);

	/// <summary>
	/// Event arguments for smart contract petition responses
	/// </summary>
	public class ContractPetitionResponseEventArgs : MessageEventArgs
	{
		private readonly Contract requestedContract;
		private readonly string petitionId;
		private readonly bool response;

		/// <summary>
		/// Event arguments for smart contract petition responses
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestedContract">Requested contract, if accepted, null if rejected.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Response">If accepted (true) or rejected (false).</param>
		public ContractPetitionResponseEventArgs(MessageEventArgs e, Contract RequestedContract, string PetitionId, bool Response)
			: base(e)
		{
			this.requestedContract = RequestedContract;
			this.petitionId = PetitionId;
			this.response = Response;
		}

		/// <summary>
		/// Requested contract, if accepted, null if rejected.
		/// </summary>
		public Contract RequestedIdentity => this.requestedContract;

		/// <summary>
		/// Petition ID
		/// </summary>
		public string PetitionId => this.petitionId;

		/// <summary>
		/// If accepted (true) or rejected (false).
		/// </summary>
		public bool Response => this.response;
	}
}
