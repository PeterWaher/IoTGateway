using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for event handlers referencing a contract.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractReferenceEventHandler(object Sender, ContractReferenceEventArgs e);

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
	}
}
