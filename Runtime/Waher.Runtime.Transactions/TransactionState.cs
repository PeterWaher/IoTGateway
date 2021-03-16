namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// State of a transaction
	/// </summary>
	public enum TransactionState
	{
		/// <summary>
		/// Transaction has been created
		/// </summary>
		Created,

		/// <summary>
		/// Transaction is being prepared
		/// </summary>
		Preparing,

		/// <summary>
		/// Transaction has been prepared
		/// </summary>
		Prepared,

		/// <summary>
		/// Transaction is being executed
		/// </summary>
		Executing,

		/// <summary>
		/// Transaction has been executed
		/// </summary>
		Executed,

		/// <summary>
		/// Transaction is being committed
		/// </summary>
		Committing,

		/// <summary>
		/// Transaction has been committed
		/// </summary>
		Committed,

		/// <summary>
		/// Transaction is being rolled back
		/// </summary>
		RollingBack,

		/// <summary>
		/// Transaction has been rolled back
		/// </summary>
		RolledBack,

		/// <summary>
		/// Transaction is in an error state. Only Rollback is permitted from this point.
		/// </summary>
		Error
	}
}
