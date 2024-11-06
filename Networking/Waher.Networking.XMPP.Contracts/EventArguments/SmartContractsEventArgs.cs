using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for smart contracts responses
	/// </summary>
	public class SmartContractsEventArgs : IqResultEventArgs
	{
		private readonly Contract[] contracts;

		/// <summary>
		/// Event arguments for smart contracts responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Contracts">Smart Contracts.</param>
		public SmartContractsEventArgs(IqResultEventArgs e, Contract[] Contracts)
			: base(e)
		{
			this.contracts = Contracts;
		}

		/// <summary>
		/// Smart Contracts
		/// </summary>
		public Contract[] Contracts => this.contracts;
	}
}
