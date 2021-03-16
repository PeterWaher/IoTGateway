using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.LifeCycle
{
	/// <summary>
	/// Database module.
	/// </summary>
	public class DatabaseModule : IModule
	{
		private static IDatabaseProvider databaseProvider = null;
		private static ILedgerProvider ledgerProvider = null;

		/// <summary>
		/// Database module.
		/// </summary>
		public DatabaseModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			if (Database.HasProvider)
				await Database.Provider.Start();

			if (Ledger.HasProvider)
				await Ledger.Provider.Start();

			databaseProvider = null;
			ledgerProvider = null;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public async Task Stop()
		{
			if (Database.HasProvider)
			{
				databaseProvider = Database.Stop();
				await databaseProvider.Stop();
			}

			if (Ledger.HasProvider)
			{
				ledgerProvider = Ledger.Stop();
				await ledgerProvider.Stop();
			}
		}

		/// <summary>
		/// Flushes any remaining data to disk.
		/// </summary>
		public static async Task Flush()
		{
			if (Database.HasProvider)
				await Database.Provider.Flush();

			if (Ledger.HasProvider)
				await Ledger.Provider.Flush();

			if (!(databaseProvider is null))
				await databaseProvider.Flush();

			if (!(ledgerProvider is null))
				await ledgerProvider.Flush();
		}

	}
}
