using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
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
