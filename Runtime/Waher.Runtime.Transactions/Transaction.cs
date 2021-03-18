using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Threading;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Asynchronous callback method.
	/// </summary>
	public delegate Task MethodAsync();

	/// <summary>
	/// Abstract base class for transactions.
	/// </summary>
	public abstract class Transaction : ITransaction
	{
		private readonly Guid id = Guid.NewGuid();
		private TransactionState state = TransactionState.Created;
		private MultiReadSingleWriteObject synchObject = new MultiReadSingleWriteObject();

		/// <summary>
		/// Abstract base class for transactions.
		/// </summary>
		public Transaction()
		{
		}

		/// <summary>
		/// Abstract base class for transactions.
		/// </summary>
		public Transaction(Guid Id)
		{
			this.id = Id;
		}

		/// <summary>
		/// ID of transaction
		/// </summary>
		public Guid Id => this.id;

		/// <summary>
		/// Transaction state.
		/// </summary>
		public TransactionState State => this.state;

		/// <summary>
		/// Disposes of the transaction.
		/// </summary>
		public void Dispose()
		{
			this.synchObject?.Dispose();
			this.synchObject = null;
		}

		/// <summary>
		/// Sets a new state for the transaction object.
		/// </summary>
		/// <param name="NewState"></param>
		private Task SetStateLocked(TransactionState NewState)
		{
			if (this.state != NewState)
			{
				this.state = NewState;
				return this.Raise(this.StateChanged);
			}
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Raises an event.
		/// </summary>
		/// <param name="EventHandler">Event handler for event.</param>
		protected async Task Raise(TransactionEventHandler EventHandler)
		{
			if (!(EventHandler is null))
			{
				try
				{
					await EventHandler(this, new TransactionEventArgs(this));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the transaction state has changed.
		/// </summary>
		public event TransactionEventHandler StateChanged;

		/// <summary>
		/// Prepares the transaction for execution. This step can be used for validation and authorization of the transaction.
		/// It must not change the underlying state.
		/// </summary>
		/// <returns>If the preparation phase is OK or not.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Created"/> state.</exception>
		public async Task<bool> Prepare()
		{
			await this.synchObject.BeginWrite();
			try
			{
				this.AssertState(TransactionState.Created);

				await this.SetStateLocked(TransactionState.Preparing);
				try
				{
					if (await this.DoPrepare())
					{
						await this.SetStateLocked(TransactionState.Prepared);
						return true;
					}
					else
					{
						await this.SetStateLocked(TransactionState.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					await this.SetStateLocked(TransactionState.Error);
					return false;
				}
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}

		/// <summary>
		/// Performs actual preparation.
		/// </summary>
		/// <returns>If the preparation phase is OK or not.</returns>
		protected abstract Task<bool> DoPrepare();

		private void AssertState(params TransactionState[] States)
		{
			foreach (TransactionState State in States)
			{
				if (this.state == State)
					return;

			}

			throw new TransactionException(this, "Unexpected state: " + this.state.ToString());
		}

		/// <summary>
		/// Executes the transaction.
		/// </summary>
		/// <returns>If the transaction was executed correctly or not.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Prepared"/> state.</exception>
		public async Task<bool> Execute()
		{
			await this.synchObject.BeginWrite();
			try
			{
				this.AssertState(TransactionState.Prepared);

				await this.SetStateLocked(TransactionState.Executing);
				try
				{
					if (await this.DoExecute())
					{
						await this.SetStateLocked(TransactionState.Executed);
						return true;
					}
					else
					{
						await this.SetStateLocked(TransactionState.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					await this.SetStateLocked(TransactionState.Error);
					return false;
				}
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}

		/// <summary>
		/// Performs actual execution.
		/// </summary>
		/// <returns>If the transaction was executed correctly or not.</returns>
		protected abstract Task<bool> DoExecute();

		/// <summary>
		/// Commits any changes made during the execution phase.
		/// </summary>
		/// <returns>If the transaction was successfully committed.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Executed"/> state.</exception>
		public async Task<bool> Commit()
		{
			await this.synchObject.BeginWrite();
			try
			{
				this.AssertState(TransactionState.Executed);

				await this.SetStateLocked(TransactionState.Committing);
				try
				{
					if (await this.DoCommit())
					{
						await this.SetStateLocked(TransactionState.Committed);
						return true;
					}
					else
					{
						await this.SetStateLocked(TransactionState.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					await this.SetStateLocked(TransactionState.Error);
					return false;
				}
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}

		/// <summary>
		/// Performs actual commit.
		/// </summary>
		/// <returns>If the transaction was successfully committed.</returns>
		protected abstract Task<bool> DoCommit();

		/// <summary>
		/// Rolls back any changes made during the execution phase.
		/// </summary>
		/// <returns>If the transaction was successfully rolled back.</returns>
		/// <exception cref="TransactionException">If the transaction not in the <see cref="TransactionState.Executed"/> 
		/// or <see cref="TransactionState.Error"/> states.</exception>
		public async Task<bool> Rollback()
		{
			await this.synchObject.BeginWrite();
			try
			{
				this.AssertState(TransactionState.Executed, TransactionState.Error);

				await this.SetStateLocked(TransactionState.RollingBack);
				try
				{
					if (await this.DoRollback())
					{
						await this.SetStateLocked(TransactionState.RolledBack);
						return true;
					}
					else
					{
						await this.SetStateLocked(TransactionState.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					await this.SetStateLocked(TransactionState.Error);
					return false;
				}
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}

		/// <summary>
		/// Performs actual rollback.
		/// </summary>
		/// <returns>If the transaction was successfully rolled back.</returns>
		protected abstract Task<bool> DoRollback();

		/// <summary>
		/// Calls <paramref name="Action"/> when the transaction is idle.
		/// </summary>
		/// <param name="Action">Callback method.</param>
		public async Task WhenIdle(MethodAsync Action)
		{
			await this.synchObject.BeginWrite();
			try
			{
				await Action();
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}

		/// <summary>
		/// Aborts the transaction.
		/// </summary>
		public async Task Abort()
		{
			await this.synchObject.BeginWrite();
			try
			{
				switch (this.state)
				{
					case TransactionState.Created:
					case TransactionState.Preparing:
					case TransactionState.Prepared:
						break;

					case TransactionState.Executing:
					case TransactionState.Executed:
					case TransactionState.Error:
						await this.SetStateLocked(TransactionState.RollingBack);
						try
						{
							await this.DoRollback();
							await this.SetStateLocked(TransactionState.RolledBack);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
							await this.SetStateLocked(TransactionState.Error);
						}
						break;

					case TransactionState.Committing:
					case TransactionState.Committed:
						break;

					case TransactionState.RollingBack:
					case TransactionState.RolledBack:
						break;
				}
			}
			finally
			{
				await this.synchObject.EndWrite();
			}
		}
	}
}
