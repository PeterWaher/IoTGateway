using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// A transaction built up of a set of sub-transactions.
	/// </summary>
	public class CompositeTransaction : Transaction
	{
		private readonly ITransaction[] transactions;
		private readonly bool parallel;

		/// <summary>
		/// A transaction built up of a set of sub-transactions.
		/// </summary>
		/// <param name="Parallel">If sub-transactions can be run in parallel (true), or in sequence (false).</param>
		/// <param name="Transactions">Subtransactions</param>
		public CompositeTransaction(bool Parallel, params ITransaction[] Transactions)
			: base()
		{
			if (Transactions is null)
				throw new ArgumentNullException("Array of transactions cannot be null.");

			this.transactions = Transactions;
			this.parallel = Parallel;
		}

		/// <summary>
		/// Performs actual preparation.
		/// </summary>
		protected override async Task<bool> DoPrepare()
		{
			if (this.parallel)
			{
				int i, c = this.transactions.Length;
				Task<bool>[] Tasks = new Task<bool>[c];

				for (i = 0; i < c; i++)
					Tasks[i] = this.transactions[i].Prepare();

				await Task.WhenAll(Tasks);

				return And(Tasks);
			}
			else
			{
				foreach (ITransaction Transaction in this.transactions)
				{
					if (!await Transaction.Prepare())
						return false;
				}

				return true;
			}
		}

		private static bool And(Task<bool>[] Tasks)
		{
			foreach (Task<bool> Task in Tasks)
			{
				if (!Task.Result)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Performs actual execution.
		/// </summary>
		protected override async Task<bool> DoExecute()
		{
			if (this.parallel)
			{
				int i, c = this.transactions.Length;
				Task<bool>[] Tasks = new Task<bool>[c];

				for (i = 0; i < c; i++)
					Tasks[i] = this.transactions[i].Execute();

				await Task.WhenAll(Tasks);

				return And(Tasks);
			}
			else
			{
				foreach (ITransaction Transaction in this.transactions)
				{
					if (!await Transaction.Execute())
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Performs actual commit.
		/// </summary>
		protected override async Task<bool> DoCommit()
		{
			if (this.parallel)
			{
				int i, c = this.transactions.Length;
				Task<bool>[] Tasks = new Task<bool>[c];

				for (i = 0; i < c; i++)
					Tasks[i] = this.transactions[i].Commit();

				await Task.WhenAll(Tasks);

				return And(Tasks);
			}
			else
			{
				foreach (ITransaction Transaction in this.transactions)
				{
					if (!await Transaction.Commit())
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Performs actual rollback.
		/// </summary>
		protected override async Task<bool> DoRollback()
		{
			if (this.parallel)
			{
				int i, c = this.transactions.Length;
				Task[] Tasks = new Task[c];

				for (i = 0; i < c; i++)
					Tasks[i] = this.transactions[i].Abort();

				await Task.WhenAll(Tasks);
			}
			else
			{
				foreach (ITransaction Transaction in this.transactions)
					await Transaction.Abort();
			}

			return true;
		}
	}
}
