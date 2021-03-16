using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Module making sure no unfinished transactions are left when system ends.
	/// </summary>
	public class TransactionModule : IModule
	{
		private readonly static List<ITransactions> transactions = new List<ITransactions>();

		/// <summary>
		/// Module making sure no unfinished transactions are left when system ends.
		/// </summary>
		public TransactionModule()
		{
		}

		/// <summary>
		/// Registers a collection of transactions with the module.
		/// </summary>
		/// <param name="Transactions">Collection of transactions.</param>
		public static void Register(ITransactions Transactions)
		{
			lock (transactions)
			{
				if (!transactions.Contains(Transactions))
					transactions.Add(Transactions);
			}
		}

		/// <summary>
		/// Unregisters a collection of transactions with the module.
		/// </summary>
		/// <param name="Transactions">Collection of transactions.</param>
		/// <returns>If the collection was found and removed.</returns>
		public static bool Unregister(ITransactions Transactions)
		{
			lock (transactions)
			{
				return transactions.Remove(Transactions);
			}
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public async Task Stop()
		{
			ITransactions[] Collections;

			lock (transactions)
			{
				Collections = transactions.ToArray();
				transactions.Clear();
			}

			foreach (ITransactions Transactions in Collections)
			{
				try
				{
					ITransaction[] Pending = await Transactions.GetTransactions();

					foreach (ITransaction T in Pending)
					{
						try
						{
							await T.Abort();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					Transactions.Dispose();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

	}
}
