using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for contract signature events
	/// </summary>
	public class ContractSignedEventArgs : ContractReferenceEventArgs
	{
		private readonly string legalId;
		private readonly string role;

		/// <summary>
		/// Event arguments for contract signature events
		/// </summary>
		/// <param name="ContractId">Contract ID</param>
		/// <param name="LegalId">Legal ID</param>
		/// <param name="Role">Role</param>
		public ContractSignedEventArgs(string ContractId, string LegalId, string Role)
			: base(ContractId)
		{
			this.legalId = LegalId;
			this.role = Role;
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
		/// ID of legal identity signing the contract, as an URI.
		/// </summary>
		public Uri LegalIdUri => ContractsClient.LegalIdUri(this.legalId);

		/// <summary>
		/// ID of legal identity signing the contract, as an URI string.
		/// </summary>
		public string LegalIdUriString => ContractsClient.LegalIdUriString(this.legalId);
	}
}
