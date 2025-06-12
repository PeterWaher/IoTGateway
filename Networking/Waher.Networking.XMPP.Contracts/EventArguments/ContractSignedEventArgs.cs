using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for contract signature events
	/// </summary>
	public class ContractSignedEventArgs : ContractReferenceEventArgs
	{
		private readonly Contract contract;
		private readonly string legalId;
		private readonly string role;
		private readonly bool signed;

		/// <summary>
		/// Event arguments for contract signature events
		/// </summary>
		/// <param name="ContractId">Contract ID</param>
		/// <param name="LegalId">Legal ID</param>
		/// <param name="Role">Role</param>
		/// <param name="Signed">If contract entered a signed state.</param>
		/// <param name="Contract">Contract that received a signature.</param>
		public ContractSignedEventArgs(string ContractId, string LegalId, string Role, 
			bool Signed, Contract Contract)
			: base(ContractId)
		{
			this.legalId = LegalId;
			this.role = Role;
			this.signed = Signed;
			this.contract = Contract;
		}

		/// <summary>
		/// ID of legal identity signing the contract.
		/// </summary>
		public string LegalId => this.legalId;

		/// <summary>
		/// Role the legal identity has signed.
		/// </summary>
		public string Role => this.role;

		/// <summary>
		/// If contract entered a signed state.
		/// </summary>
		public bool Signed => this.signed;

		/// <summary>
		/// Contract that received a signature.
		/// </summary>
		public Contract Contract => this.contract;

		/// <summary>
		/// ID of legal identity signing the contract, as an URI.
		/// </summary>
		public Uri LegalIdUri => ContractsClient.LegalIdUri(this.legalId);

		/// <summary>
		/// ID of legal identity signing the contract, as an URI string.
		/// </summary>
		public string LegalIdUriString => ContractsClient.LegalIdUriString(this.legalId);
	}
}
