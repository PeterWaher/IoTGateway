using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for smart contract proposal events.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractProposalEventHandler(object Sender, ContractProposalEventArgs e);

	/// <summary>
	/// Event arguments for smart contract proposals
	/// </summary>
	public class ContractProposalEventArgs : MessageEventArgs
	{
		private readonly string contractId;
		private readonly string role;
		private readonly string message;

		/// <summary>
		/// Event arguments for smart contract petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role in proposed contract.</param>
		/// <param name="Message">Optional message to present to recipient.</param>
		public ContractProposalEventArgs(MessageEventArgs e, string ContractId, string Role, string Message)
			: base(e)
		{
			this.contractId = ContractId;
			this.role = Role;
			this.message = Message;
		}

		/// <summary>
		/// ID of proposed contract.
		/// </summary>
		public string ContractId => this.contractId;

		/// <summary>
		/// Proposed role in proposed contract.
		/// </summary>
		public string Role => this.role;

		/// <summary>
		/// Optional message to present to recipient.
		/// </summary>
		public string MessageText => this.message;
	}
}
