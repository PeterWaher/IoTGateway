using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for events referencing a contract.
	/// </summary>
	public class ContractReferenceEventArgs : EventArgs
	{
		private readonly string contractId;

		/// <summary>
		/// Event arguments for events referencing a contract.
		/// </summary>
		public ContractReferenceEventArgs(string ContractId)
			: base()
		{
			this.contractId = ContractId;
		}

		/// <summary>
		/// ID of contract being signed.
		/// </summary>
		public string ContractId => this.contractId;

		/// <summary>
		/// ID of contract being signed, as an URI.
		/// </summary>
		public Uri ContractIdUri => ContractsClient.ContractIdUri(this.contractId);

		/// <summary>
		/// ID of contract being signed, as an URI string.
		/// </summary>
		public string ContractIdUriString => ContractsClient.ContractIdUriString(this.contractId);
	}
}
