using System.Threading.Tasks;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

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
		private readonly byte[] key;
		private readonly SymmetricCipherAlgorithms keyAlgorithm;

		/// <summary>
		/// Event arguments for smart contract petitions
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="ContractId">ID of proposed contract.</param>
		/// <param name="Role">Proposed role in proposed contract.</param>
		/// <param name="Message">Optional message to present to recipient.</param>
		/// <param name="Key">Shared secret used to decrypt confidential parameters.</param>
		/// <param name="KeyAlgorithm">Encryption algorithm to encrypt and decrypt confidential parameters.</param>
		public ContractProposalEventArgs(MessageEventArgs e, string ContractId, string Role, string Message,
			byte[] Key, SymmetricCipherAlgorithms KeyAlgorithm)
			: base(e)
		{
			this.contractId = ContractId;
			this.role = Role;
			this.message = Message;
			this.key = Key;
			this.keyAlgorithm = KeyAlgorithm;
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

		/// <summary>
		/// If the proposed contract uses a shared secret.
		/// </summary>
		public bool HasSharedSecret => !(this.key is null);

		/// <summary>
		/// Shared secret used to decrypt confidential parameters.
		/// </summary>
		public byte[] Key => this.key;

		/// <summary>
		/// Encryption algorithm to encrypt and decrypt confidential parameters.
		/// </summary>
		public SymmetricCipherAlgorithms KeyAlgorithm => this.keyAlgorithm;
	}
}
