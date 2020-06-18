using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for smart contract callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SmartContractEventHandler(object Sender, SmartContractEventArgs e);

	/// <summary>
	/// Event arguments for smart contract responses
	/// </summary>
	public class SmartContractEventArgs : IqResultEventArgs
	{
		private readonly Contract contract;

		/// <summary>
		/// Event arguments for smart contract responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Contract">Smart Contract.</param>
		public SmartContractEventArgs(IqResultEventArgs e, Contract Contract)
			: base(e)
		{
			this.contract = Contract;
		}

		/// <summary>
		/// Smart Contract
		/// </summary>
		public Contract Contract => this.contract;
	}
}
