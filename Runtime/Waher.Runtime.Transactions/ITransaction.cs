using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Interface for transactions
	/// </summary>
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// ID of transaction
		/// </summary>
		Guid Id
		{
			get;
		}

		/// <summary>
		/// Transaction state.
		/// </summary>
		TransactionState State
		{
			get;
		}

		/// <summary>
		/// Event raised when the transaction state has changed.
		/// </summary>
		event TransactionEventHandler StateChanged;

		/// <summary>
		/// Prepares the transaction for execution. This step can be used for validation and authorization of the transaction.
		/// It must not change the underlying state.
		/// </summary>
		/// <returns>If the preparation phase is OK or not.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Created"/> state.</exception>
		Task<bool> Prepare();

		/// <summary>
		/// Executes the transaction.
		/// </summary>
		/// <returns>If the transaction was executed correctly or not.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Prepared"/> state.</exception>
		Task<bool> Execute();

		/// <summary>
		/// Commits any changes made during the execution phase.
		/// </summary>
		/// <returns>If the transaction was successfully committed.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Executed"/> state.</exception>
		Task<bool> Commit();

		/// <summary>
		/// Rolls back any changes made during the execution phase.
		/// </summary>
		/// <returns>If the transaction was successfully rolled back.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.RolledBack"/> state.</exception>
		Task<bool> Rollback();

		/// <summary>
		/// Aborts the transaction.
		/// </summary>
		Task Abort();
	}
}
