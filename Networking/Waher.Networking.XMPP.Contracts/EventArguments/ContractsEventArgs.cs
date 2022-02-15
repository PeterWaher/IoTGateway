using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for Contracts callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ContractsEventHandler(object Sender, ContractsEventArgs e);

	/// <summary>
	/// Event arguments for Contracts responses
	/// </summary>
	public class ContractsEventArgs : IqResultEventArgs
	{
		private readonly Contract[] contracts;

		/// <summary>
		/// Event arguments for ID References responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Contracts">Contracts.</param>
		public ContractsEventArgs(IqResultEventArgs e, Contract[] Contracts)
			: base(e)
		{
			this.contracts = Contracts;
		}

		/// <summary>
		/// Contracts
		/// </summary>
		public Contract[] Contracts => this.contracts;
	}
}
