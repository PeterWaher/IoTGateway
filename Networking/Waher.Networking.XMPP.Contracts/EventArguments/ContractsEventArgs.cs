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
		private readonly string[] references;

		/// <summary>
		/// Event arguments for ID References responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Contracts">Contracts.</param>
		/// <param name="ReferencesOnly">References to contracts that could not be returned.</param>
		public ContractsEventArgs(IqResultEventArgs e, Contract[] Contracts, string[] ReferencesOnly)
			: base(e)
		{
			this.contracts = Contracts;
			this.references = ReferencesOnly;
		}

		/// <summary>
		/// Contracts
		/// </summary>
		public Contract[] Contracts => this.contracts;

		/// <summary>
		/// References to contracts that could not be returned.
		/// </summary>
		public string[] References => this.references;
	}
}
