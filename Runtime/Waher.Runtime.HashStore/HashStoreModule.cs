using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.HashStore.HashObjects;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.HashStore
{
	/// <summary>
	/// Module controling the life-cycle of persisted hash values.
	/// </summary>
	[Singleton]
	public class HashStoreModule : IModule
	{
		private static bool running = false;
		private static Timer timer = null;

		/// <summary>
		/// If the module is running or not.
		/// </summary>
		public static bool Running => running;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			int OneHour = 1000 * 60 * 60;

			running = true;
			timer = new Timer(DeleteExpiredHashes, null, OneHour, OneHour);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			running = false;

			timer?.Dispose();
			timer = null;

			return Task.CompletedTask;
		}

		private static async void DeleteExpiredHashes(object _)
		{
			try
			{
				await Database.Delete<PersistedHash>(
					new FilterFieldLesserOrEqualTo("ExpiresUtc", DateTime.UtcNow));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
