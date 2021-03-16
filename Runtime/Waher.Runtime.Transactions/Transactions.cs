using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Maintains a collection of active transactions.
	/// </summary>
	/// <typeparam name="T">Type of transaction managed by the class</typeparam>
	public class Transactions<T> : ITransactions
		where T : ITransaction
	{
		private readonly Cache<Guid, T> transactions;

		/// <summary>
		/// Maintains a collection of active transactions.
		/// </summary>
		/// <param name="TransactionTimeout">Maximum time before a transaction needs to complete or fail.</param>
		public Transactions(TimeSpan TransactionTimeout)
		{
			this.transactions = new Cache<Guid, T>(int.MaxValue, TransactionTimeout, TransactionTimeout);
			this.transactions.Removed += Transactions_Removed;

			TransactionModule.Register(this);
		}

		private async void Transactions_Removed(object Sender, CacheItemEventArgs<Guid, T> e)
		{
			try
			{
				T Transaction = e.Value;

				if (Transaction.State != TransactionState.Committed &&
					Transaction.State != TransactionState.RolledBack)
				{
					await Transaction.Abort();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Rolls back any pending transactions and disposes of the object.
		/// </summary>
		public void Dispose()
		{
			TransactionModule.Unregister(this);
		
			this.transactions.Removed -= Transactions_Removed;
			this.transactions.Clear();
			this.transactions.Dispose();
		}

		/// <summary>
		/// Creates a new transaction
		/// </summary>
		/// <param name="Arguments">Constructor arguments.</param>
		/// <returns>New transaction</returns>
		public T CreateNew(params object[] Arguments)
		{
			T Transaction = Types.Instantiate<T>(false, Arguments);
			this.transactions.Add(Transaction.Id, Transaction);
			return Transaction;
		}

		/// <summary>
		/// Register a transaction created elsewhere with the collection.
		/// </summary>
		/// <param name="Transaction">Transaction already created.</param>
		public void Register(T Transaction)
		{
			this.transactions.Add(Transaction.Id, Transaction);
		}

		/// <summary>
		/// Prepares a transaction in the collection.
		/// </summary>
		/// <param name="TransactionId">Transaction ID</param>
		/// <returns>If a transaction with the corresponding ID was found, and successfully prepared.</returns>
		public async Task<bool> Prepare(Guid TransactionId)
		{
			try
			{
				if (!this.transactions.TryGetValue(TransactionId, out T Transaction))
					return false;

				return await Transaction.Prepare();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Executes a transaction in the collection.
		/// </summary>
		/// <param name="TransactionId">Transaction ID</param>
		/// <returns>If a transaction with the corresponding ID was found, and successfully executed.</returns>
		public async Task<bool> Execute(Guid TransactionId)
		{
			try
			{
				if (!this.transactions.TryGetValue(TransactionId, out T Transaction))
					return false;

				return await Transaction.Execute();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Cimmits a transaction in the collection.
		/// </summary>
		/// <param name="TransactionId">Transaction ID</param>
		/// <returns>If a transaction with the corresponding ID was found, and successfully committed.</returns>
		public async Task<bool> Commit(Guid TransactionId)
		{
			try
			{
				if (!this.transactions.TryGetValue(TransactionId, out T Transaction))
					return false;

				if (!await Transaction.Commit())
					return false;

				this.transactions.Remove(TransactionId);

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Prepares a transaction in the collection.
		/// </summary>
		/// <param name="TransactionId">Transaction ID</param>
		/// <returns>If a transaction with the corresponding ID was found, and successfully prepared.</returns>
		public async Task<bool> Rollback(Guid TransactionId)
		{
			try
			{
				if (!this.transactions.TryGetValue(TransactionId, out T Transaction))
					return false;

				if (!await Transaction.Rollback())
					return false;

				this.transactions.Remove(TransactionId);

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Gets pending transactions.
		/// </summary>
		public Task<ITransaction[]> GetTransactions()
		{
			T[] Transactions = this.transactions.GetValues();
			int i, c = Transactions.Length;
			ITransaction[] Result = new ITransaction[c];

			for (i = 0; i < c; i++)
				Result[i] = (ITransaction)Transactions[i];

			return Task.FromResult<ITransaction[]>(Result);
		}
	}
}
