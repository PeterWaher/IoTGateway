﻿using System.Threading.Tasks;

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
		private readonly string clientEndpoint;

		/// <summary>
		/// Event arguments for smart contract petition responses
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RequestedContract">Requested contract, if accepted, null if rejected.</param>
		/// <param name="PetitionId">Petition ID. Identifies the petition.</param>
		/// <param name="Response">If accepted (true) or rejected (false).</param>
		/// <param name="ClientEndpoint">Remote endpoint of remote party client.</param>
		public ContractPetitionResponseEventArgs(MessageEventArgs e, Contract RequestedContract, string PetitionId, 
			bool Response, string ClientEndpoint)
			: base(e)
		{
			this.requestedContract = RequestedContract;
			this.petitionId = PetitionId;
			this.response = Response;
			this.clientEndpoint = ClientEndpoint;
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
	}
}
