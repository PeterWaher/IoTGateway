using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Delegate for transaction events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task TransactionEventHandler(object Sender, TransactionEventArgs e);

	/// <summary>
	/// Event arguments for transaction events.
	/// </summary>
	public class TransactionEventArgs : EventArgs
	{
		private readonly ITransaction transaction;

		/// <summary>
		/// Event arguments for transaction events.
		/// </summary>
		/// <param name="Transaction">Reference to transaction object.</param>
		public TransactionEventArgs(ITransaction Transaction)
			: base()
		{
			this.transaction = Transaction;
		}

		/// <summary>
		/// Reference to transaction object.
		/// </summary>
		public ITransaction Transaction => this.transaction;
	}
}
