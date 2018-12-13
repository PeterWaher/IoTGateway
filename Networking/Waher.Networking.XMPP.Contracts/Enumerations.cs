using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Recognized contract states
	/// </summary>
	public enum ContractState
	{
		/// <summary>
		/// The contract has been proposed as a new contract.
		/// It needs to be revied and approved by the Trust Provider before it can be used as a template or be signed.
		/// </summary>
		Proposed,

		/// <summary>
		/// The contract has been deemed incomplete, inconsistent, or otherwise faulty.
		/// A rejected contract cannot be used as a template or be signed.
		/// A rejected contract can be updated by the creator, and thus be put in a Proposed state again.
		/// </summary>
		Rejected,

		/// <summary>
		/// The contract has been reviewed and approved. It is still not signed, but can act as a template for other contracts.
		/// </summary>
		Approved,

		/// <summary>
		/// The contract is being signed. Not all reuired roles have signed however, and the contract is not legally binding.
		/// </summary>
		BeingSigned,

		/// <summary>
		/// The contract has been signed by all required parties, and is legally binding.
		/// </summary>
		Signed,

		/// <summary>
		/// The contract has been explicitly obsoleted by its owner, or by the Trust Provider.
		/// </summary>
		Obsoleted,

		/// <summary>
		/// The contract has been explicitly deleted by its owner, or by the Trust Provider.
		/// </summary>
		Deleted
	}

	/// <summary>
	/// Visibility types for contracts.
	/// </summary>
	public enum ContractVisibility
	{
		/// <summary>
		/// Contract is only accessible to the creator, and any parts in the contract.
		/// </summary>
		CreatorAndParts,

		/// <summary>
		/// Contract is accessible to the creator of the contract, any parts in the contract, and any account on the Trust Provider server domain.
		/// </summary>
		DomainAndParts,

		/// <summary>
		/// Contract is accessible by everyone requesting it. It is not searchable.
		/// </summary>
		Public,

		/// <summary>
		/// Contract is accessible by everyone requesting it. It is also searchable.
		/// </summary>
		PublicSearchable
	}
}
