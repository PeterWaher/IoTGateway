using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Counters
{
	/// <summary>
	/// Module controling the life-cycle of counters.
	/// </summary>
	[Singleton]
	public class CounterModule : IModule
	{
		private static bool running = false;

		/// <summary>
		/// If the module is running or not.
		/// </summary>
		public static bool Running => running;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			running = true;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public async Task Stop()
		{
			running = false;

			await RuntimeCounters.FlushAsync();
		}

	}
}
